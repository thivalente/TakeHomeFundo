using FundoTakeHome.Api.Features.ApprovedApplicationDelivery.Application.Models;

namespace FundoTakeHome.Api.Features.ApprovedApplicationDelivery.Application.Interfaces;

public interface IOutboxMessageStore
{
    Task<OutboxMessageCandidate?> ClaimNextAsync(DateTimeOffset now, DateTimeOffset lockedUntil, Guid lockId, CancellationToken cancellationToken);

    Task<ApprovedApplicationDeliveryPayload?> LoadDeliveryAsync(OutboxEventEnvelope envelope, CancellationToken cancellationToken);

    Task<bool> MarkProcessedAsync(Guid messageId, Guid lockId, DateTimeOffset processedAt, CancellationToken cancellationToken);

    Task<bool> MarkFailedAsync(Guid messageId, Guid lockId, string error, DateTimeOffset now, bool permanent, CancellationToken cancellationToken);
}
