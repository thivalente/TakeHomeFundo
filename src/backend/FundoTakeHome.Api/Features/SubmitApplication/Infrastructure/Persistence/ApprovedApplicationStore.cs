using FundoTakeHome.Api.Features.SubmitApplication.Application.Interfaces;
using FundoTakeHome.Api.Features.SubmitApplication.Domain.Common.Enums;
using FundoTakeHome.Api.Features.SubmitApplication.Domain.Entities;
using FundoTakeHome.Api.Features.SubmitApplication.Domain.ValueObjects;
using FundoTakeHome.Api.Infrastructure.Persistence;
using FundoTakeHome.Api.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;

namespace FundoTakeHome.Api.Features.SubmitApplication.Infrastructure.Persistence;

public sealed class ApprovedApplicationStore(FundoTakeHomeDbContext dbContext) : IApprovedApplicationStore
{
    public async Task<Customer?> FindCustomerBySsnAsync(Ssn ssn, CancellationToken cancellationToken)
    {
        var customer = await dbContext.Customers.SingleOrDefaultAsync(current => current.Ssn == ssn, cancellationToken);

        if (customer is null)
            return null;

        var application = await dbContext.Applications.SingleOrDefaultAsync(current => current.CustomerId == customer.Id, cancellationToken);

        if (application is not null)
            customer.AttachApplication(application);

        return customer;
    }

    public void AddCustomer(Customer customer)
    {
        dbContext.Customers.Add(customer);
    }

    public void AddLoanApplication(LoanApplication application)
    {
        dbContext.Applications.Add(application);
    }

    public void AddOutboxMessage(Customer customer, LoanApplication application, EntityOperationEnum operation, DateTimeOffset occurredAt)
    {
        var eventId = Guid.CreateVersion7();
        var approvedApplicationEvent = new ApprovedApplicationEvent(eventId, OutboxEventTypeEnum.ApplicationApproved, customer.Id, application.Id, operation, occurredAt);

        dbContext.OutboxMessages.Add(new OutboxMessage
        {
            Attempts = 0,
            EventType = approvedApplicationEvent.EventType.ToString(),
            Id = approvedApplicationEvent.EventId,
            OccurredAt = approvedApplicationEvent.OccurredAt,
            Payload = approvedApplicationEvent.ToPayload(),
            Status = OutboxStatusEnum.Pending
        });
    }

    public void UpdateCustomer(Customer customer)
    {
        dbContext.Customers.Update(customer);
    }

    public void UpdateLoanApplication(LoanApplication application)
    {
        dbContext.Applications.Update(application);
    }
}
