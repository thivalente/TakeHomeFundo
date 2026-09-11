using FundoTakeHome.Api.Features.ApprovedApplicationDelivery.Application.Interfaces;
using FundoTakeHome.Api.Features.ApprovedApplicationDelivery.Application.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using FundoTakeHome.Api.Common.Interfaces;
using FundoTakeHome.Api.Features.SubmitApplication.Domain.Common.Enums;
using FundoTakeHome.Api.Features.ApprovedApplicationDelivery.Application;


namespace FundoTakeHome.Tests.Features.ApprovedApplicationDelivery;

public sealed class OutboxProcessorTests
{
    [Fact]
    public async Task ShouldSendCreatedEventAndMarkMessageAsProcessed()
    {
        var candidate = CreateCandidate(attempts: 1);
        var delivery = CreateDelivery(EntityOperationEnum.Created);
        var store = CreateStore(candidate, delivery);
        var integration = new Mock<IApprovedApplicationIntegration>();
        integration.Setup(client => client.SendAsync(It.IsAny<ApprovedApplicationDeliveryPayload>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(IntegrationDeliveryResult.Success());

        var processor = CreateProcessor(store, integration);

        (await processor.ProcessNextAsync(CancellationToken.None)).ShouldBeTrue();

        integration.Verify(client => client.SendAsync(It.Is<ApprovedApplicationDeliveryPayload>(value => value.Operation == EntityOperationEnum.Created), It.IsAny<CancellationToken>()), Times.Once);
        store.Verify(value => value.MarkProcessedAsync(candidate.MessageId, candidate.LockId, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ShouldSendUpdatedEvent()
    {
        var candidate = CreateCandidate(attempts: 1);
        var store = CreateStore(candidate, CreateDelivery(EntityOperationEnum.Updated));
        var integration = new Mock<IApprovedApplicationIntegration>();
        integration.Setup(client => client.SendAsync(It.IsAny<ApprovedApplicationDeliveryPayload>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(IntegrationDeliveryResult.Success());

        var processor = CreateProcessor(store, integration);

        await processor.ProcessNextAsync(CancellationToken.None);

        integration.Verify(client => client.SendAsync(It.Is<ApprovedApplicationDeliveryPayload>(value => value.Operation == EntityOperationEnum.Updated), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ShouldScheduleRetryAfterDeliveryFailure()
    {
        var candidate = CreateCandidate(attempts: 2);
        var store = CreateStore(candidate, CreateDelivery(EntityOperationEnum.Created));
        var integration = new Mock<IApprovedApplicationIntegration>();
        integration.Setup(client => client.SendAsync(It.IsAny<ApprovedApplicationDeliveryPayload>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(IntegrationDeliveryResult.Failure("service unavailable"));

        var processor = CreateProcessor(store, integration);

        await processor.ProcessNextAsync(CancellationToken.None);

        store.Verify(value => value.MarkFailedAsync(candidate.MessageId, candidate.LockId, "service unavailable", It.IsAny<DateTimeOffset>(), false, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ShouldScheduleRetryAfterTimeout()
    {
        var candidate = CreateCandidate(attempts: 1);
        var store = CreateStore(candidate, CreateDelivery(EntityOperationEnum.Created));
        var integration = new Mock<IApprovedApplicationIntegration>();
        integration.Setup(client => client.SendAsync(It.IsAny<ApprovedApplicationDeliveryPayload>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var processor = CreateProcessor(store, integration);

        await processor.ProcessNextAsync(CancellationToken.None);

        store.Verify(value => value.MarkFailedAsync(candidate.MessageId, candidate.LockId, "The external integration timed out.", It.IsAny<DateTimeOffset>(), false, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ShouldScheduleRetryAfterNetworkError()
    {
        var candidate = CreateCandidate(attempts: 1);
        var store = CreateStore(candidate, CreateDelivery(EntityOperationEnum.Created));
        var integration = new Mock<IApprovedApplicationIntegration>();
        integration.Setup(client => client.SendAsync(It.IsAny<ApprovedApplicationDeliveryPayload>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("network down"));

        var processor = CreateProcessor(store, integration);

        await processor.ProcessNextAsync(CancellationToken.None);

        store.Verify(value => value.MarkFailedAsync(candidate.MessageId, candidate.LockId, "network down", It.IsAny<DateTimeOffset>(), false, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ShouldScheduleRetryAfterNonSuccessIntegrationResult()
    {
        var candidate = CreateCandidate(attempts: 1);
        var store = CreateStore(candidate, CreateDelivery(EntityOperationEnum.Created));
        var integration = new Mock<IApprovedApplicationIntegration>();
        integration.Setup(client => client.SendAsync(It.IsAny<ApprovedApplicationDeliveryPayload>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(IntegrationDeliveryResult.Failure("status 503"));

        var processor = CreateProcessor(store, integration);

        await processor.ProcessNextAsync(CancellationToken.None);

        store.Verify(value => value.MarkFailedAsync(candidate.MessageId, candidate.LockId, "status 503", It.IsAny<DateTimeOffset>(), false, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ShouldUseFixedFiveSecondBackoff()
    {
        var candidate = CreateCandidate(attempts: 1);
        var store = CreateStore(candidate, CreateDelivery(EntityOperationEnum.Created));
        var integration = new Mock<IApprovedApplicationIntegration>();
        integration.Setup(client => client.SendAsync(It.IsAny<ApprovedApplicationDeliveryPayload>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(IntegrationDeliveryResult.Failure("temporary failure"));

        var processor = CreateProcessor(store, integration);

        await processor.ProcessNextAsync(CancellationToken.None);

        store.Verify(value => value.MarkFailedAsync(candidate.MessageId, candidate.LockId, "temporary failure", new DateTimeOffset(2026, 9, 10, 12, 0, 0, TimeSpan.Zero), false, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ShouldMarkMessageAsFailedAfterThirdAttempt()
    {
        var candidate = CreateCandidate(attempts: 3);
        var store = CreateStore(candidate, CreateDelivery(EntityOperationEnum.Created));
        var integration = new Mock<IApprovedApplicationIntegration>();
        integration.Setup(client => client.SendAsync(It.IsAny<ApprovedApplicationDeliveryPayload>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(IntegrationDeliveryResult.Failure("permanent failure"));

        var processor = CreateProcessor(store, integration);

        await processor.ProcessNextAsync(CancellationToken.None);

        store.Verify(value => value.MarkFailedAsync(candidate.MessageId, candidate.LockId, "permanent failure", It.IsAny<DateTimeOffset>(), true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ShouldMarkMessageAsFailedWhenReferencedDataIsMissing()
    {
        var candidate = CreateCandidate(attempts: 1);
        var store = CreateStore(candidate, delivery: null);
        var integration = new Mock<IApprovedApplicationIntegration>();

        var processor = CreateProcessor(store, integration);

        await processor.ProcessNextAsync(CancellationToken.None);

        integration.Verify(client => client.SendAsync(It.IsAny<ApprovedApplicationDeliveryPayload>(), It.IsAny<CancellationToken>()), Times.Never);
        store.Verify(value => value.MarkFailedAsync(candidate.MessageId, candidate.LockId, It.Is<string>(error => error.Contains("missing Customer or Application")), It.IsAny<DateTimeOffset>(), true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ShouldPermanentlyFailInvalidPayloadWithoutCallingIntegration()
    {
        var candidate = new OutboxMessageCandidate(Guid.NewGuid(), "{invalid", 1, Guid.NewGuid());
        var store = CreateStore(candidate, CreateDelivery(EntityOperationEnum.Created));
        var integration = new Mock<IApprovedApplicationIntegration>();

        var processor = CreateProcessor(store, integration);

        await processor.ProcessNextAsync(CancellationToken.None);

        integration.Verify(client => client.SendAsync(It.IsAny<ApprovedApplicationDeliveryPayload>(), It.IsAny<CancellationToken>()), Times.Never);
        store.Verify(value => value.MarkFailedAsync(candidate.MessageId, candidate.LockId, "The outbox payload is invalid.", It.IsAny<DateTimeOffset>(), true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ShouldPreserveEventIdAndOperationInDeliveryPayload()
    {
        var eventId = Guid.Parse("00000000-0000-0000-0000-000000000011");
        var candidate = CreateCandidate(eventId, EntityOperationEnum.Updated, attempts: 1);
        var store = CreateStore(candidate, CreateDelivery(EntityOperationEnum.Updated, eventId));
        var integration = new Mock<IApprovedApplicationIntegration>();
        ApprovedApplicationDeliveryPayload? sent = null;
        integration.Setup(client => client.SendAsync(It.IsAny<ApprovedApplicationDeliveryPayload>(), It.IsAny<CancellationToken>()))
            .Callback<ApprovedApplicationDeliveryPayload, CancellationToken>((delivery, _) => sent = delivery)
            .ReturnsAsync(IntegrationDeliveryResult.Success());

        var processor = CreateProcessor(store, integration);

        await processor.ProcessNextAsync(CancellationToken.None);

        sent.ShouldNotBeNull();
        sent!.EventId.ShouldBe(eventId);
        sent.Operation.ShouldBe(EntityOperationEnum.Updated);
    }

    [Fact]
    public async Task ShouldNotPersistCompletionWhenStoreRejectsCurrentLock()
    {
        var candidate = CreateCandidate(attempts: 1);
        var store = CreateStore(candidate, CreateDelivery(EntityOperationEnum.Created));
        store.Setup(value => value.MarkProcessedAsync(candidate.MessageId, candidate.LockId, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var integration = new Mock<IApprovedApplicationIntegration>();
        integration.Setup(client => client.SendAsync(It.IsAny<ApprovedApplicationDeliveryPayload>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(IntegrationDeliveryResult.Success());
        var unitOfWork = new Mock<IUnitOfWork>();

        var processor = CreateProcessor(store, integration, unitOfWork);

        await processor.ProcessNextAsync(CancellationToken.None);

        unitOfWork.Verify(value => value.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ShouldNotPersistFailureWhenStoreRejectsCurrentLock()
    {
        var candidate = CreateCandidate(attempts: 1);
        var store = CreateStore(candidate, CreateDelivery(EntityOperationEnum.Created));
        store.Setup(value => value.MarkFailedAsync(candidate.MessageId, candidate.LockId, It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var integration = new Mock<IApprovedApplicationIntegration>();
        integration.Setup(client => client.SendAsync(It.IsAny<ApprovedApplicationDeliveryPayload>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(IntegrationDeliveryResult.Failure("temporary failure"));
        var unitOfWork = new Mock<IUnitOfWork>();

        var processor = CreateProcessor(store, integration, unitOfWork);

        await processor.ProcessNextAsync(CancellationToken.None);

        unitOfWork.Verify(value => value.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ShouldRequestClaimWithThirtySecondLease()
    {
        var candidate = CreateCandidate(attempts: 1);
        var store = CreateStore(candidate, delivery: null);
        DateTimeOffset? claimedAt = null;
        DateTimeOffset? leaseExpiresAt = null;
        store.Setup(value => value.ClaimNextAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Callback<DateTimeOffset, DateTimeOffset, Guid, CancellationToken>((now, lockedUntil, _, _) =>
            {
                claimedAt = now;
                leaseExpiresAt = lockedUntil;
            })
            .ReturnsAsync((OutboxMessageCandidate?)null);
        var integration = new Mock<IApprovedApplicationIntegration>();

        var processor = CreateProcessor(store, integration);

        (await processor.ProcessNextAsync(CancellationToken.None)).ShouldBeFalse();

        claimedAt.ShouldBe(new DateTimeOffset(2026, 9, 10, 12, 0, 0, TimeSpan.Zero));
        leaseExpiresAt.ShouldBe(new DateTimeOffset(2026, 9, 10, 12, 0, 30, TimeSpan.Zero));
    }

    private static Mock<IOutboxMessageStore> CreateStore(OutboxMessageCandidate candidate, ApprovedApplicationDeliveryPayload? delivery)
    {
        var store = new Mock<IOutboxMessageStore>();
        store.Setup(value => value.ClaimNextAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidate);
        store.Setup(value => value.LoadDeliveryAsync(It.IsAny<OutboxEventEnvelope>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(delivery);
        store.Setup(value => value.MarkProcessedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        store.Setup(value => value.MarkFailedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        return store;
    }

    private static OutboxProcessor CreateProcessor(Mock<IOutboxMessageStore> store, Mock<IApprovedApplicationIntegration> integration, Mock<IUnitOfWork>? unitOfWork = null)
    {
        var clock = new Mock<IDateTimeProvider>();
        clock.SetupGet(value => value.UtcNow).Returns(new DateTimeOffset(2026, 9, 10, 12, 0, 0, TimeSpan.Zero));
        var logger = new Mock<ILogger<OutboxProcessor>>();
        unitOfWork ??= new Mock<IUnitOfWork>();
        unitOfWork.Setup(value => value.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return new OutboxProcessor(store.Object, unitOfWork.Object, integration.Object, clock.Object, logger.Object);
    }

    private static OutboxMessageCandidate CreateCandidate(int attempts) =>
        CreateCandidate(Guid.Parse("00000000-0000-0000-0000-000000000001"), EntityOperationEnum.Created, attempts);

    private static OutboxMessageCandidate CreateCandidate(Guid eventId, EntityOperationEnum operation, int attempts) =>
        new(Guid.CreateVersion7(), $"{{\"eventId\":\"{eventId}\",\"eventType\":\"ApplicationApproved\",\"customerId\":\"00000000-0000-0000-0000-000000000002\",\"applicationId\":\"00000000-0000-0000-0000-000000000003\",\"operation\":\"{operation}\",\"occurredAt\":\"2026-09-10T12:00:00Z\"}}", attempts, Guid.CreateVersion7());

    private static ApprovedApplicationDeliveryPayload CreateDelivery(EntityOperationEnum operation, Guid? eventId = null) =>
        new(eventId ?? Guid.CreateVersion7(), operation, Guid.CreateVersion7(), "123456789", "Ada", "Lovelace", "1 Analytical Engine Way", "NY", "Computing Company", Guid.CreateVersion7(), 1000m);
}
