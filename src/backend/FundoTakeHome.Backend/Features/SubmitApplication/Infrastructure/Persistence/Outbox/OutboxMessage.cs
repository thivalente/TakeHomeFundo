namespace FundoTakeHome.Backend.Features.SubmitApplication.Infrastructure.Persistence.Outbox;

public sealed class OutboxMessage
{
    public int Attempts { get; set; }

    public string? Error { get; set; }

    public string EventType { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public DateTimeOffset OccurredAt { get; set; }

    public DateTimeOffset? NextAttemptAt { get; set; }

    public string Payload { get; set; } = string.Empty;

    public DateTimeOffset? ProcessedAt { get; set; }

    public OutboxStatusEnum Status { get; set; } = OutboxStatusEnum.Pending;
}
