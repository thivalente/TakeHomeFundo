namespace FundoTakeHome.Worker;

public sealed class Worker(ILogger<Worker> logger) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("FundoTakeHome worker started.");
        return Task.CompletedTask;
    }
}
