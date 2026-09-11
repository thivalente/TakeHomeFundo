namespace FundoTakeHome.Api.Infrastructure.Persistence.Outbox;

public sealed class OutboxMessage
{
    public int Attempts { get; set; }

    public string? LastError { get; set; }

    public string EventType { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public Guid? LockId { get; set; }

    public DateTimeOffset? LockedUntil { get; set; }

    public DateTimeOffset OccurredAt { get; set; }

    public DateTimeOffset? NextAttemptAt { get; set; }

    public string Payload { get; set; } = string.Empty;

    public DateTimeOffset? ProcessedAt { get; set; }

    public OutboxStatusEnum Status { get; set; } = OutboxStatusEnum.Pending;
}
