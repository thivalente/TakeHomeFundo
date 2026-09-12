using FundoTakeHome.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FundoTakeHome.Tests.Features.SubmitApplication.Support;

public sealed class SubmitApplicationApiFactory : WebApplicationFactory<Program>, IAsyncDisposable
{
    private readonly string databasePath = Path.Combine(Path.GetTempPath(), $"fundotakehome-tests-{Guid.NewGuid():N}.db");

    public string ConnectionString => $"Data Source={databasePath}";

    public FundoTakeHomeDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<FundoTakeHomeDbContext>().UseSqlite(ConnectionString).Options;
        return new FundoTakeHomeDbContext(options);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Default"] = ConnectionString,
            ["ExternalService:BaseUrl"] = "http://localhost:1/"
        }));

        builder.ConfigureServices(services =>
        {
            var hostedService = services.FirstOrDefault(descriptor => descriptor.ImplementationType == typeof(FundoTakeHome.Api.BackgroundServices.OutboxBackgroundService));

            if (hostedService is not null)
                services.Remove(hostedService);
        });
    }

    public new async ValueTask DisposeAsync()
    {
        Dispose();
        SqliteConnection.ClearAllPools();
        await Task.CompletedTask;

        if (File.Exists(databasePath))
            File.Delete(databasePath);
    }
}
