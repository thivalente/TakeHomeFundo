using FundoTakeHome.Backend.Features.SubmitApplication.Domain.Entities;
using FundoTakeHome.Backend.Features.SubmitApplication.Domain.Enums;
using FundoTakeHome.Backend.Features.SubmitApplication.Domain.ValueObjects;

namespace FundoTakeHome.Backend.Features.SubmitApplication.Application.Interfaces;

public interface IApprovedApplicationStore
{
    Task<Customer?> FindCustomerBySsnAsync(Ssn ssn, CancellationToken cancellationToken);

    void AddCustomer(Customer customer);

    void AddLoanApplication(LoanApplication application);

    void AddOutboxMessage(Customer customer, LoanApplication application, EntityOperationEnum operation, DateTimeOffset occurredAt);

    void UpdateCustomer(Customer customer);

    void UpdateLoanApplication(LoanApplication application);
}
