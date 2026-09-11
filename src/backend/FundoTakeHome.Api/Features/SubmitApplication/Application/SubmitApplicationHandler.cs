using ErrorOr;
using FundoTakeHome.Api.Features.SubmitApplication.Application.Interfaces;
using FundoTakeHome.Api.Features.SubmitApplication.Domain.Common.Errors;
using FundoTakeHome.Api.Features.SubmitApplication.Domain.Entities;
using FundoTakeHome.Api.Features.SubmitApplication.Domain.ValueObjects;
using FundoTakeHome.Api.Features.SubmitApplication.Application.DecisionRules;
using FundoTakeHome.Api.Features.SubmitApplication.Application.Models;
using FundoTakeHome.Api.Common.Interfaces;
using FundoTakeHome.Api.Features.SubmitApplication.Domain.Common.Enums;

namespace FundoTakeHome.Api.Features.SubmitApplication.Application;

public sealed class SubmitApplicationHandler(IApprovedApplicationStore store, IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider, IDecisionRuleEngine decisionRuleEngine)
{
    public async Task<ErrorOr<SubmitApplicationResult>> HandleAsync(SubmitApplicationRequest request, CancellationToken cancellationToken)
    {
        var customerResult = Customer.Create(request.Ssn, request.FirstName, request.LastName, request.Address, request.State, request.CompanyName);

        if (customerResult.IsError)
            return customerResult.Errors;

        var decisionErrors = await decisionRuleEngine.EvaluateAsync(new DecisionRuleInput(customerResult.Value), cancellationToken);

        if (decisionErrors.Count > 0)
            return decisionErrors.ToList();

        var applicationResult = LoanApplication.Create(customerResult.Value.Id, request.RequestedAmount);

        if (applicationResult.IsError)
            return applicationResult.Errors;

        var customer = await store.FindCustomerBySsnAsync(customerResult.Value.Ssn, cancellationToken);
        var operation = customer is null ? EntityOperationEnum.Created : EntityOperationEnum.Updated;
        LoanApplication? application;

        if (customer is null)
        {
            application = applicationResult.Value;
            customer = customerResult.Value.AttachApplication(application);

            store.AddCustomer(customer);
            store.AddLoanApplication(application);
        }
        else
        {
            var updateResult = customer.Update(request.FirstName, request.LastName, request.Address, request.State, request.CompanyName);

            if (updateResult.IsError)
                return updateResult.Errors;

            if (customer.Application is null)
                return CustomerErrors.ApplicationNotFound;

            application = customer.Application!;
            var applicationUpdateResult = application.Update(request.RequestedAmount);

            if (applicationUpdateResult.IsError)
                return applicationUpdateResult.Errors;

            store.UpdateCustomer(customer);
            store.UpdateLoanApplication(application);
        }

        var occurredAt = dateTimeProvider.UtcNow;
        store.AddOutboxMessage(customer, application, operation, occurredAt);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new SubmitApplicationResult(application.Id.Value, customer.Id.Value, ApplicationStatusEnum.Approved.ToString().ToLowerInvariant(), operation == EntityOperationEnum.Created);
    }

}
