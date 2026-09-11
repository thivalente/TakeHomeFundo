using ErrorOr;
using FundoTakeHome.Api.Features.SubmitApplication.Application.DecisionRules;
using FundoTakeHome.Api.Features.SubmitApplication.Application.Models;
using FundoTakeHome.Api.Features.SubmitApplication.Application.Interfaces;
using FundoTakeHome.Api.Features.SubmitApplication.Domain.Common.Errors;
using FundoTakeHome.Api.Features.SubmitApplication.Domain.Entities;
using FundoTakeHome.Api.Features.SubmitApplication.Domain.ValueObjects;
using Moq;
using Shouldly;
using FundoTakeHome.Api.Common.Interfaces;
using FundoTakeHome.Api.Features.SubmitApplication.Application;
using FundoTakeHome.Api.Features.SubmitApplication.Domain.Common.Enums;

namespace FundoTakeHome.Tests.Features.SubmitApplication;

public sealed class SubmitApplicationHandlerTests
{
    private static readonly DateTimeOffset FixedDate = new(2026, 9, 10, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task ShouldCreateCustomerApplicationAndEvent_WhenCustomerIsNew()
    {
        var store = new Mock<IApprovedApplicationStore>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var dateTimeProvider = new Mock<IDateTimeProvider>();
        dateTimeProvider.SetupGet(provider => provider.UtcNow).Returns(FixedDate);
        store.Setup(repository => repository.FindCustomerBySsnAsync(It.IsAny<Ssn>(), It.IsAny<CancellationToken>())).ReturnsAsync((Customer?)null);

        var result = await CreateHandler(store, unitOfWork, dateTimeProvider).HandleAsync(CreateRequest(), CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.Created.ShouldBeTrue();
        store.Verify(repository => repository.FindCustomerBySsnAsync(It.Is<Ssn>(ssn => ssn.Value == "123456789"), CancellationToken.None), Times.Once);
        store.Verify(repository => repository.AddCustomer(It.IsAny<Customer>()), Times.Once);
        store.Verify(repository => repository.AddLoanApplication(It.IsAny<LoanApplication>()), Times.Once);
        store.Verify(repository => repository.AddOutboxMessage(It.IsAny<Customer>(), It.IsAny<LoanApplication>(), EntityOperationEnum.Created, FixedDate), Times.Once);
        store.Verify(repository => repository.UpdateCustomer(It.IsAny<Customer>()), Times.Never);
        store.Verify(repository => repository.UpdateLoanApplication(It.IsAny<LoanApplication>()), Times.Never);
        unitOfWork.Verify(work => work.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task ShouldUpdateCustomerAndApplicationWithoutChangingIds_WhenCustomerReturns()
    {
        var existingCustomer = Customer.Create("123456789", "Jane", "Doe", "1 Main Street", "CA", "Fundo").Value!;
        var existingApplication = LoanApplication.Create(existingCustomer.Id, 100m).Value!;
        existingCustomer.AttachApplication(existingApplication);

        var store = new Mock<IApprovedApplicationStore>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var dateTimeProvider = new Mock<IDateTimeProvider>();
        dateTimeProvider.SetupGet(provider => provider.UtcNow).Returns(FixedDate);
        store.Setup(repository => repository.FindCustomerBySsnAsync(It.IsAny<Ssn>(), It.IsAny<CancellationToken>())).ReturnsAsync(existingCustomer);

        var result = await CreateHandler(store, unitOfWork, dateTimeProvider).HandleAsync(CreateRequest() with { FirstName = "Updated", RequestedAmount = 200m }, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.Created.ShouldBeFalse();
        result.Value.CustomerId.ShouldBe(existingCustomer.Id.Value);
        result.Value.ApplicationId.ShouldBe(existingApplication.Id.Value);
        store.Verify(repository => repository.FindCustomerBySsnAsync(It.IsAny<Ssn>(), CancellationToken.None), Times.Once);
        store.Verify(repository => repository.AddCustomer(It.IsAny<Customer>()), Times.Never);
        store.Verify(repository => repository.AddLoanApplication(It.IsAny<LoanApplication>()), Times.Never);
        store.Verify(repository => repository.UpdateCustomer(existingCustomer), Times.Once);
        store.Verify(repository => repository.UpdateLoanApplication(existingApplication), Times.Once);
        store.Verify(repository => repository.AddOutboxMessage(existingCustomer, existingApplication, EntityOperationEnum.Updated, FixedDate), Times.Once);
        unitOfWork.Verify(work => work.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task ShouldReturnDomainErrorsAndAvoidPersistence_WhenRequestIsInvalid()
    {
        var store = new Mock<IApprovedApplicationStore>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var dateTimeProvider = new Mock<IDateTimeProvider>();

        var result = await CreateHandler(store, unitOfWork, dateTimeProvider).HandleAsync(CreateRequest() with { FirstName = " " }, CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.Errors.ShouldContain(error => error.Code == "customer.first_name_required");
        store.Verify(repository => repository.FindCustomerBySsnAsync(It.IsAny<Ssn>(), It.IsAny<CancellationToken>()), Times.Never);
        unitOfWork.Verify(work => work.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ShouldReturnStateDenialAndAvoidPersistence_WhenApplicationIsFromNewYork()
    {
        var store = new Mock<IApprovedApplicationStore>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var dateTimeProvider = new Mock<IDateTimeProvider>();
        var decisionRuleEngine = new Mock<IDecisionRuleEngine>();
        decisionRuleEngine.Setup(engine => engine.EvaluateAsync(It.IsAny<DecisionRuleInput>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult<IReadOnlyList<Error>>([ApplicationDenialErrors.StateIsNewYork]));

        var result = await CreateHandler(store, unitOfWork, dateTimeProvider, decisionRuleEngine).HandleAsync(CreateRequest(), CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.Errors.ShouldContain(error => error.Code == ApplicationDenialErrors.StateIsNewYork.Code);
        store.Verify(repository => repository.FindCustomerBySsnAsync(It.IsAny<Ssn>(), It.IsAny<CancellationToken>()), Times.Never);
        store.Verify(repository => repository.AddCustomer(It.IsAny<Customer>()), Times.Never);
        store.Verify(repository => repository.AddLoanApplication(It.IsAny<LoanApplication>()), Times.Never);
        store.Verify(repository => repository.AddOutboxMessage(It.IsAny<Customer>(), It.IsAny<LoanApplication>(), It.IsAny<EntityOperationEnum>(), It.IsAny<DateTimeOffset>()), Times.Never);
        unitOfWork.Verify(work => work.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static SubmitApplicationHandler CreateHandler(Mock<IApprovedApplicationStore> store, Mock<IUnitOfWork> unitOfWork, Mock<IDateTimeProvider> dateTimeProvider) =>
        CreateHandler(store, unitOfWork, dateTimeProvider, CreateDecisionRuleEngine());

    private static SubmitApplicationHandler CreateHandler(Mock<IApprovedApplicationStore> store, Mock<IUnitOfWork> unitOfWork, Mock<IDateTimeProvider> dateTimeProvider, Mock<IDecisionRuleEngine> decisionRuleEngine) =>
        new(store.Object, unitOfWork.Object, dateTimeProvider.Object, decisionRuleEngine.Object);

    private static Mock<IDecisionRuleEngine> CreateDecisionRuleEngine()
    {
        var decisionRuleEngine = new Mock<IDecisionRuleEngine>();
        decisionRuleEngine.Setup(engine => engine.EvaluateAsync(It.IsAny<DecisionRuleInput>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult<IReadOnlyList<Error>>([]));
        return decisionRuleEngine;
    }

    private static SubmitApplicationRequest CreateRequest() => new("Jane", "Doe", "1 Main Street", "CA", "Fundo", 1000m, "123-45-6789");
}
