namespace FundoTakeHome.Api.BackgroundServices;

using FundoTakeHome.Api.Features.ApprovedApplicationDelivery.Application;

public sealed class OutboxBackgroundService(IServiceScopeFactory scopeFactory, ILogger<OutboxBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("FundoTakeHome worker started.");

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = scopeFactory.CreateScope();
                    var processor = scope.ServiceProvider.GetRequiredService<OutboxProcessor>();

                    if (!await processor.ProcessNextAsync(stoppingToken))
                        break;
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "Outbox processing cycle failed. The worker will retry on the next polling cycle.");
                    break;
                }
            }
        }
    }
}
