using FundoTakeHome.Api.Features.SubmitApplication.Application.Interfaces;
using FundoTakeHome.Api.Features.SubmitApplication.Domain.Entities;
using FundoTakeHome.Api.Features.SubmitApplication.Domain.Enums;
using FundoTakeHome.Api.Features.SubmitApplication.Domain.ValueObjects;
using FundoTakeHome.Api.Features.SubmitApplication.Infrastructure.Persistence.Outbox;
using FundoTakeHome.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FundoTakeHome.Api.Features.SubmitApplication.Infrastructure.Persistence;

public sealed class ApprovedApplicationStore(FundoTakeHomeDbContext dbContext) : IApprovedApplicationStore
{
    public Task<Customer?> FindCustomerBySsnAsync(Ssn ssn, CancellationToken cancellationToken) =>
        dbContext.Customers
                    .Include(customer => customer.Application)
                    .SingleOrDefaultAsync(customer => customer.Ssn.Value == ssn.Value, cancellationToken);

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
