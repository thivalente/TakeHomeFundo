using System.Text.Json;
using System.Text.Json.Serialization;
using FundoTakeHome.Api.Features.SubmitApplication.Domain.Common.Enums;

namespace FundoTakeHome.Api.Features.ApprovedApplicationDelivery.Application.Models;

public sealed record OutboxEventEnvelope(Guid EventId, OutboxEventTypeEnum EventType, Guid CustomerId, Guid ApplicationId, EntityOperationEnum Operation, DateTimeOffset OccurredAt)
{
    public static bool TryParse(string payload, out OutboxEventEnvelope? envelope)
    {
        OutboxEventEnvelope? parsedEnvelope;

        try
        {
            parsedEnvelope = JsonSerializer.Deserialize<OutboxEventEnvelope>(payload, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            });
        }
        catch (JsonException)
        {
            envelope = null;
            return false;
        }
        catch (NotSupportedException)
        {
            envelope = null;
            return false;
        }

        if (parsedEnvelope is null
            || parsedEnvelope.EventType != OutboxEventTypeEnum.ApplicationApproved
            || (parsedEnvelope.Operation != EntityOperationEnum.Created && parsedEnvelope.Operation != EntityOperationEnum.Updated)
            || parsedEnvelope.EventId == Guid.Empty
            || parsedEnvelope.CustomerId == Guid.Empty
            || parsedEnvelope.ApplicationId == Guid.Empty
            || parsedEnvelope.OccurredAt == default)
        {
            envelope = null;
            return false;
        }

        envelope = parsedEnvelope;
        return true;
    }
}
