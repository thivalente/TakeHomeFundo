using FundoTakeHome.Api.Common.Exceptions;
using FundoTakeHome.Api.Common.Interfaces;
using FundoTakeHome.Api.Features.ApprovedApplicationDelivery.Application.Interfaces;
using FundoTakeHome.Api.Features.ApprovedApplicationDelivery.Application.Models;

namespace FundoTakeHome.Api.Features.ApprovedApplicationDelivery.Application;

public sealed class OutboxProcessor(IOutboxMessageStore store, IUnitOfWork unitOfWork, IApprovedApplicationIntegration integration, IDateTimeProvider dateTimeProvider, ILogger<OutboxProcessor> logger)
{
    // The lease gives a worker temporary ownership of a message while it is being delivered. If the worker crashes or becomes unresponsive, another worker can reclaim the message after the lease expires.
    private static readonly TimeSpan LeaseDuration = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan DeliveryTimeout = TimeSpan.FromSeconds(5);

    public async Task<bool> ProcessNextAsync(CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.UtcNow;
        var lockId = Guid.CreateVersion7(); // The lock ID uniquely identifies this claim, so only the worker that claimed the message can complete it.
        var leaseExpiresAt = now.Add(LeaseDuration);

        var message = await store.ClaimNextAsync(now, leaseExpiresAt, lockId, cancellationToken);

        if (message is null)
            return false;

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (ConcurrencyConflictException)
        {
            logger.LogDebug("Outbox message {MessageId} was claimed by another worker before this worker could save the lease.", message.MessageId);
            return true;
        }

        logger.LogInformation("Outbox message {MessageId} claimed for processing on attempt {Attempt}.", message.MessageId, message.Attempts);

        try
        {
            if (!OutboxEventEnvelope.TryParse(message.Payload, out var envelope))
            {
                const string error = "The outbox payload is invalid.";
                logger.LogError("Outbox message {MessageId} was permanently failed. Reason: {Reason}", message.MessageId, error);
                await CompleteFailureAsync(message, error, true, cancellationToken);
                return true;
            }

            var delivery = await store.LoadDeliveryAsync(envelope!, cancellationToken);

            if (delivery is null)
            {
                const string error = "The event references missing Customer or Application data.";
                logger.LogError("Outbox message {MessageId} was permanently failed. Reason: {Reason}", message.MessageId, error);
                await CompleteFailureAsync(message, error, true, cancellationToken);
                return true;
            }

            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(DeliveryTimeout);
            var result = await integration.SendAsync(delivery, timeout.Token);

            if (result.Succeeded)
            {
                logger.LogInformation("Outbox message {MessageId} was delivered successfully on attempt {Attempt}.", message.MessageId, message.Attempts);

                if (await store.MarkProcessedAsync(message.MessageId, message.LockId, dateTimeProvider.UtcNow, cancellationToken))
                    await unitOfWork.SaveChangesAsync(cancellationToken);
            }
            else
                await RecordFailureAsync(message, result.Error ?? "The external integration returned a failure.", cancellationToken);
        }
        catch (ConcurrencyConflictException)
        {
            logger.LogDebug("Outbox message {MessageId} could not be processed because its lease was lost before persistence.", message.MessageId);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            await RecordFailureAsync(message, "The external integration timed out.", cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Outbox message {MessageId} failed during delivery.", message.MessageId);
            await RecordFailureAsync(message, exception.Message, cancellationToken);
        }

        return true;
    }

    private async Task RecordFailureAsync(OutboxMessageCandidate message, string error, CancellationToken cancellationToken)
    {
        var permanent = message.Attempts >= 3;

        if (permanent)
            logger.LogError("Outbox message {MessageId} was permanently failed on attempt {Attempt}. Reason: {Reason}", message.MessageId, message.Attempts, error);
        else
            logger.LogWarning("Outbox message {MessageId} failed on attempt {Attempt} and will be retried. Reason: {Reason}", message.MessageId, message.Attempts, error);

        await CompleteFailureAsync(message, error, permanent, cancellationToken);
    }

    private async Task CompleteFailureAsync(OutboxMessageCandidate message, string error, bool permanent, CancellationToken cancellationToken)
    {
        try
        {
            if (await store.MarkFailedAsync(message.MessageId, message.LockId, error, dateTimeProvider.UtcNow, permanent, cancellationToken))
                await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (ConcurrencyConflictException)
        {
            logger.LogDebug("Outbox message {MessageId} could not be failed because its lease was lost before persistence.", message.MessageId);
        }
    }
}
