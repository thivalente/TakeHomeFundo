using FundoTakeHome.Backend.Features.SubmitApplication.Domain.Entities;
using FundoTakeHome.Backend.Features.SubmitApplication.Domain.Enums;
using FundoTakeHome.Backend.Features.SubmitApplication.Domain.ValueObjects;
using FundoTakeHome.Backend.Features.SubmitApplication.Domain.ValueObjects.Identifiers;
using FundoTakeHome.Backend.Features.SubmitApplication.Infrastructure.Persistence.Outbox;
using Shouldly;

namespace FundoTakeHome.Tests.Features.SubmitApplication;

public sealed class SubmitApplicationDomainTests
{
    [Fact]
    public void ShouldReturnSpecificErrors_WhenCustomerRequiredDataIsMissing()
    {
        var result = Customer.Create("123456789", " ", "", " ", "CA", "");

        result.IsError.ShouldBeTrue();
        result.Errors.Count.ShouldBe(4);
        result.Errors.Select(error => error.Code).ShouldContain("customer.first_name_required");
        result.Errors.Select(error => error.Code).ShouldContain("customer.last_name_required");
        result.Errors.Select(error => error.Code).ShouldContain("customer.address_required");
        result.Errors.Select(error => error.Code).ShouldContain("customer.company_name_required");
    }

    [Fact]
    public void ShouldReturnSpecificErrors_WhenCustomerDataExceedsMaximumLength()
    {
        var result = Customer.Create("123456789", new string('a', 101), new string('a', 101), new string('a', 301), "CA", new string('a', 201));

        result.IsError.ShouldBeTrue();
        result.Errors.Count.ShouldBe(4);
        result.Errors.ShouldContain(error => error.Code == "customer.first_name_too_long");
        result.Errors.ShouldContain(error => error.Code == "customer.last_name_too_long");
        result.Errors.ShouldContain(error => error.Code == "customer.address_too_long");
        result.Errors.ShouldContain(error => error.Code == "customer.company_name_too_long");
    }

    [Fact]
    public void ShouldUpdateCustomer_WhenDataIsValid()
    {
        var customer = CreateCustomer();

        var result = customer.Update(" Updated  Name ", "Doe", " New  Address ", "NY", "New  Company");

        result.IsError.ShouldBeFalse();
        customer.FirstName.ShouldBe("Updated Name");
        customer.Address.ShouldBe("New Address");
        customer.State.Value.ShouldBe("NY");
    }

    [Fact]
    public void ShouldNotChangeCustomer_WhenUpdateDataIsInvalid()
    {
        var customer = CreateCustomer();
        var previousName = customer.FirstName;

        var result = customer.Update(new string('a', 101), "Doe", "Address", "CA", "Company");

        result.IsError.ShouldBeTrue();
        customer.FirstName.ShouldBe(previousName);
    }

    [Fact]
    public void ShouldCreateApprovedLoanApplication_WhenRequestedAmountIsValid()
    {
        var result = LoanApplication.Create(CustomerId.New(), 100m);

        result.IsError.ShouldBeFalse();
        result.Value.Id.Value.ShouldNotBe(Guid.Empty);
        result.Value.Status.ShouldBe(ApplicationStatusEnum.Approved);
    }

    [Fact]
    public void ShouldRejectLoanApplication_WhenCustomerIdIsMissing()
    {
        var result = LoanApplication.Create(null, 100m);

        result.IsError.ShouldBeTrue();
        result.Errors.ShouldContain(error => error.Code == "loan_application.customer_id_required");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1_000_000)]
    public void ShouldRejectLoanApplication_WhenRequestedAmountIsInvalid(decimal amount)
    {
        var result = LoanApplication.Create(CustomerId.New(), amount);

        result.IsError.ShouldBeTrue();
        result.Errors.ShouldContain(error => error.Code == "requested_amount.invalid");
    }

    [Fact]
    public void ShouldGenerateCustomerIdentifierInsideFactory_WhenCustomerIsCreated()
    {
        var customer = CreateCustomer();

        customer.Id.Value.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void ShouldUpdateLoanApplicationAndKeepApprovedStatus_WhenAmountChanges()
    {
        var application = LoanApplication.Create(CustomerId.New(), 100m).Value!;

        var result = application.Update(200m);

        result.IsError.ShouldBeFalse();
        application.RequestedAmount.Value.ShouldBe(200m);
        application.Status.ShouldBe(ApplicationStatusEnum.Approved);
    }

    [Fact]
    public void ShouldRejectLoanApplicationUpdate_WhenRequestedAmountIsInvalid()
    {
        var application = LoanApplication.Create(CustomerId.New(), 100m).Value!;

        var result = application.Update(0m);

        result.IsError.ShouldBeTrue();
        result.Errors.ShouldContain(error => error.Code == "requested_amount.invalid");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1_000_000)]
    public void ShouldRejectRequestedAmountOutsideExclusiveRange_WhenCreatingValueObject(decimal amount)
    {
        RequestedAmount.From(amount).IsError.ShouldBeTrue();
    }

    [Fact]
    public void ShouldNormalizeSsn_WhenFormattedValueIsProvided()
    {
        var result = Ssn.From(" 123-45-6789 ");

        result.IsError.ShouldBeFalse();
        result.Value.Value.ShouldBe("123456789");
    }

    [Fact]
    public void ShouldRejectSsn_WhenFormatIsInvalid()
    {
        Ssn.From("123").IsError.ShouldBeTrue();
    }

    [Fact]
    public void ShouldNormalizeState_WhenValidValueIsProvided()
    {
        UsState.From(" ny ").Value!.Value.ShouldBe("NY");
    }

    [Fact]
    public void ShouldRejectState_WhenCodeIsInvalid()
    {
        UsState.From("XX").IsError.ShouldBeTrue();
    }

    [Fact]
    public void ShouldRejectEmptyIdentifiers_WhenGuidIsEmpty()
    {
        CustomerId.From(Guid.Empty).IsError.ShouldBeTrue();
        LoanApplicationId.From(Guid.Empty).IsError.ShouldBeTrue();
    }

    [Fact]
    public void ShouldCreatePendingOutboxMessage_WhenMessageIsCreated()
    {
        var message = new OutboxMessage();

        message.Status.ShouldBe(OutboxStatusEnum.Pending);
        message.Attempts.ShouldBe(0);
    }

    private static Customer CreateCustomer() => Customer.Create("123456789", "Jane", "Doe", "Address", "CA", "Company").Value!;
}
