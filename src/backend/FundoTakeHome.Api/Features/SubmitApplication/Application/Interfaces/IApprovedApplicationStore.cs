using FundoTakeHome.Api.Features.SubmitApplication.Domain.Common.Enums;
using FundoTakeHome.Api.Features.SubmitApplication.Domain.Entities;
using FundoTakeHome.Api.Features.SubmitApplication.Domain.ValueObjects;

namespace FundoTakeHome.Api.Features.SubmitApplication.Application.Interfaces;

public interface IApprovedApplicationStore
{
    Task<Customer?> FindCustomerBySsnAsync(Ssn ssn, CancellationToken cancellationToken);

    void AddCustomer(Customer customer);

    void AddLoanApplication(LoanApplication application);

    void AddOutboxMessage(Customer customer, LoanApplication application, EntityOperationEnum operation, DateTimeOffset occurredAt);

    void UpdateCustomer(Customer customer);

    void UpdateLoanApplication(LoanApplication application);
}
