using System.Text.Json;
using FundoTakeHome.Backend.Features.SubmitApplication.Domain.Enums;
using FundoTakeHome.Backend.Features.SubmitApplication.Domain.ValueObjects.Identifiers;

namespace FundoTakeHome.Backend.Features.SubmitApplication.Infrastructure.Persistence.Outbox;

public sealed record ApprovedApplicationEvent(Guid EventId, OutboxEventTypeEnum EventType, CustomerId CustomerId, LoanApplicationId ApplicationId, EntityOperationEnum Operation, DateTimeOffset OccurredAt)
{
    public string ToPayload() =>
        JsonSerializer.Serialize(new
        {
            eventId = EventId,
            eventType = EventType.ToString(),
            customerId = CustomerId.Value,
            applicationId = ApplicationId.Value,
            operation = Operation.ToString(),
            occurredAt = OccurredAt
        });
}
