namespace FundoTakeHome.Api.Features.ApprovedApplicationDelivery.Application.Models;

public sealed record OutboxMessageCandidate(Guid MessageId, string Payload, int Attempts, Guid LockId);
