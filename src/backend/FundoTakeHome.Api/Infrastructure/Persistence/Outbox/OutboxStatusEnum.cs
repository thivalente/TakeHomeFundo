namespace FundoTakeHome.Api.Infrastructure.Persistence.Outbox;

public enum OutboxStatusEnum
{
    Pending,
    Processing,
    Processed,
    Failed
}
