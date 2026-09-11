using FundoTakeHome.Api.Features.ApprovedApplicationDelivery.Application.Interfaces;
using FundoTakeHome.Api.Features.ApprovedApplicationDelivery.Application.Models;
using FundoTakeHome.Api.Features.SubmitApplication.Domain.ValueObjects.Identifiers;
using FundoTakeHome.Api.Infrastructure.Persistence;
using FundoTakeHome.Api.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FundoTakeHome.Api.Features.ApprovedApplicationDelivery.Infrastructure.Persistence;

public sealed class OutboxMessageStore(FundoTakeHomeDbContext dbContext) : IOutboxMessageStore
{
    public async Task<OutboxMessageCandidate?> ClaimNextAsync(DateTimeOffset now, DateTimeOffset lockedUntil, Guid lockId, CancellationToken cancellationToken)
    {
        var messages = await dbContext.OutboxMessages
            .Where(message => message.Status == OutboxStatusEnum.Pending || message.Status == OutboxStatusEnum.Processing)
            .ToListAsync(cancellationToken);

        // SQLite cannot sort DateTimeOffset in SQL, so the query is split and ordering happens in memory.
        var message = messages
            .Where(EligibleAt(now).Compile())
            .OrderBy(message => message.OccurredAt)
            .FirstOrDefault();

        if (message is null)
            return null;

        var attempts = message.Attempts + 1;
        message.Status = OutboxStatusEnum.Processing;
        message.LockId = lockId;
        message.LockedUntil = lockedUntil;
        message.Attempts = attempts;

        return new OutboxMessageCandidate(message.Id, message.Payload, attempts, lockId);
    }

    public async Task<ApprovedApplicationDeliveryPayload?> LoadDeliveryAsync(OutboxEventEnvelope envelope, CancellationToken cancellationToken)
    {
        var customerId = CustomerId.From(envelope.CustomerId).Value!;
        var applicationId = LoanApplicationId.From(envelope.ApplicationId).Value!;

        var customer = await dbContext.Customers.AsNoTracking().SingleOrDefaultAsync(current => current.Id == customerId, cancellationToken);

        if (customer is not null)
        {
            var application = await dbContext.Applications.AsNoTracking().SingleOrDefaultAsync(current => current.CustomerId == customerId && current.Id == applicationId, cancellationToken);

            if (application is not null)
                customer.AttachApplication(application);
        }

        if (customer?.Application is null)
            return null;

        return new ApprovedApplicationDeliveryPayload(
            envelope.EventId,
            envelope.Operation,
            customer.Id.Value,
            customer.Ssn.Value,
            customer.FirstName,
            customer.LastName,
            customer.Address,
            customer.State.Value,
            customer.CompanyName,
            customer.Application.Id.Value,
            customer.Application.RequestedAmount.Value);
    }

    public async Task<bool> MarkProcessedAsync(Guid messageId, Guid lockId, DateTimeOffset processedAt, CancellationToken cancellationToken)
    {
        var message = await dbContext.OutboxMessages.SingleOrDefaultAsync(message => message.Id == messageId && message.Status == OutboxStatusEnum.Processing && message.LockId == lockId, cancellationToken);

        if (message is null)
            return false;

        message.Status = OutboxStatusEnum.Processed;
        message.ProcessedAt = processedAt;
        message.LockedUntil = null;
        message.LockId = null;
        message.LastError = null;
        message.NextAttemptAt = null;

        return true;
    }

    public async Task<bool> MarkFailedAsync(Guid messageId, Guid lockId, string error, DateTimeOffset now, bool permanent, CancellationToken cancellationToken)
    {
        var message = await dbContext.OutboxMessages
            .SingleOrDefaultAsync(message => message.Id == messageId && message.Status == OutboxStatusEnum.Processing && message.LockId == lockId, cancellationToken);

        if (message is null)
            return false;

        message.Status = permanent ? OutboxStatusEnum.Failed : OutboxStatusEnum.Pending;
        message.LastError = error;
        message.NextAttemptAt = permanent ? null : now.AddSeconds(5);
        message.LockedUntil = null;
        message.LockId = null;

        return true;
    }

    // Business rule: a message can be sent when it is waiting for its first or next attempt,
    // or when the temporary ownership of the previous worker has expired.
    private static Expression<Func<OutboxMessage, bool>> EligibleAt(DateTimeOffset now) =>
        message =>
            (message.Status == OutboxStatusEnum.Pending && (message.NextAttemptAt == null || message.NextAttemptAt <= now)) ||
            (message.Status == OutboxStatusEnum.Processing && message.LockedUntil != null && message.LockedUntil <= now);
}
