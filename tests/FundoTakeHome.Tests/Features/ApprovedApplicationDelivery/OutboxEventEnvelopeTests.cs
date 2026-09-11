using System.Text.Json;
using FundoTakeHome.Api.Features.ApprovedApplicationDelivery.Application.Models;
using FundoTakeHome.Api.Features.SubmitApplication.Domain.Common.Enums;
using Shouldly;

namespace FundoTakeHome.Tests.Features.ApprovedApplicationDelivery;

public sealed class OutboxEventEnvelopeTests
{
    [Fact]
    public void ShouldParseCreatedApplicationApprovedEnvelope()
    {
        var payload = CreatePayload(OutboxEventTypeEnum.ApplicationApproved, EntityOperationEnum.Created);

        OutboxEventEnvelope.TryParse(payload, out var envelope).ShouldBeTrue();
        envelope!.EventType.ShouldBe(OutboxEventTypeEnum.ApplicationApproved);
        envelope.Operation.ShouldBe(EntityOperationEnum.Created);
    }

    [Fact]
    public void ShouldParseUpdatedApplicationApprovedEnvelope()
    {
        var payload = CreatePayload(OutboxEventTypeEnum.ApplicationApproved, EntityOperationEnum.Updated);

        OutboxEventEnvelope.TryParse(payload, out var envelope).ShouldBeTrue();
        envelope!.Operation.ShouldBe(EntityOperationEnum.Updated);
    }

    [Fact]
    public void ShouldRejectUnknownEventType()
    {
        var payload = CreatePayload("UnknownEvent", "Created");

        OutboxEventEnvelope.TryParse(payload, out var envelope).ShouldBeFalse();
        envelope.ShouldBeNull();
    }

    [Fact]
    public void ShouldRejectUnknownOperation()
    {
        var payload = CreatePayload("ApplicationApproved", "Deleted");

        OutboxEventEnvelope.TryParse(payload, out var envelope).ShouldBeFalse();
        envelope.ShouldBeNull();
    }

    [Fact]
    public void ShouldRejectEmptyIdentifiers()
    {
        var payload = CreatePayload(OutboxEventTypeEnum.ApplicationApproved, EntityOperationEnum.Created, Guid.Empty);

        OutboxEventEnvelope.TryParse(payload, out var envelope).ShouldBeFalse();
        envelope.ShouldBeNull();
    }

    [Fact]
    public void ShouldRejectMissingOccurredAt()
    {
        var payload = JsonSerializer.Serialize(new
        {
            eventId = Guid.NewGuid(),
            eventType = "ApplicationApproved",
            customerId = Guid.NewGuid(),
            applicationId = Guid.NewGuid(),
            operation = "Created"
        });

        OutboxEventEnvelope.TryParse(payload, out var envelope).ShouldBeFalse();
        envelope.ShouldBeNull();
    }

    [Fact]
    public void ShouldRejectMalformedJsonWithoutThrowing()
    {
        Should.NotThrow(() => OutboxEventEnvelope.TryParse("{invalid", out _));
        OutboxEventEnvelope.TryParse("{invalid", out var envelope).ShouldBeFalse();
        envelope.ShouldBeNull();
    }

    private static string CreatePayload(OutboxEventTypeEnum eventType, EntityOperationEnum operation, Guid? eventId = null) =>
        CreatePayload(eventType.ToString(), operation.ToString(), eventId);

    private static string CreatePayload(string eventType, string operation, Guid? eventId = null) =>
        JsonSerializer.Serialize(new
        {
            eventId = eventId ?? Guid.NewGuid(),
            eventType,
            customerId = Guid.NewGuid(),
            applicationId = Guid.NewGuid(),
            operation,
            occurredAt = new DateTimeOffset(2026, 9, 11, 12, 0, 0, TimeSpan.Zero)
        });
}
