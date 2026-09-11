using FluentValidation;
using FundoTakeHome.Api.BackgroundServices;
using FundoTakeHome.Api.Common.Interfaces;
using FundoTakeHome.Api.Features.ApprovedApplicationDelivery.Application;
using FundoTakeHome.Api.Features.ApprovedApplicationDelivery.Application.Interfaces;
using FundoTakeHome.Api.Features.ApprovedApplicationDelivery.Infrastructure.ExternalServices;
using FundoTakeHome.Api.Features.ApprovedApplicationDelivery.Infrastructure.Persistence;
using FundoTakeHome.Api.Features.SubmitApplication.Application;
using FundoTakeHome.Api.Features.SubmitApplication.Application.DecisionRules;
using FundoTakeHome.Api.Features.SubmitApplication.Application.Interfaces;
using FundoTakeHome.Api.Features.SubmitApplication.Application.Validators;
using FundoTakeHome.Api.Features.SubmitApplication.Infrastructure.Persistence;
using FundoTakeHome.Api.Infrastructure.Persistence;
using FundoTakeHome.Api.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;

namespace FundoTakeHome.Api.Setup;

public static class DependencyInjection
{
    public static IServiceCollection AddServicesConfig(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddCorsConfig(configuration)
            .AddApplicationServices()
            .AddDecisionRules()
            .AddPersistence(configuration)
            .AddIntegrations(configuration);

        return services;
    }

    private static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddValidatorsFromAssemblyContaining<SubmitApplicationRequestValidator>();
        services.AddScoped<SubmitApplicationHandler>();

        return services;
    }

    private static IServiceCollection AddCorsConfig(this IServiceCollection services, IConfiguration configuration)
    {
        var corsOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .GetChildren()
            .Select(section => section.Value)
            .OfType<string>()
            .ToArray();

        services.AddCors(options => options.AddPolicy("Frontend", policy => policy
            .WithOrigins(corsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()));

        return services;
    }

    private static IServiceCollection AddDecisionRules(this IServiceCollection services)
    {
        services.AddScoped<IDecisionRule<DecisionRuleInput>, StateIsNyRule>();
        services.AddScoped<IDecisionRule<DecisionRuleInput>, BlacklistedSsnRule>();
        services.AddScoped<IDecisionRuleEngine, DecisionRuleEngine>();

        return services;
    }

    private static IServiceCollection AddIntegrations(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddScoped<IOutboxMessageStore, OutboxMessageStore>();
        services.AddScoped<OutboxProcessor>();
        services.AddHttpClient<IApprovedApplicationIntegration, ApprovedApplicationHttpClient>((serviceProvider, client) =>
        {
            var baseUrl = serviceProvider.GetRequiredService<IConfiguration>()["ExternalService:BaseUrl"] ?? throw new InvalidOperationException("External service base URL is not configured.");

            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(5);
        });
        services.AddHostedService<OutboxBackgroundService>();

        return services;
    }

    private static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<FundoTakeHomeDbContext>(options => options.UseSqlite(configuration.GetConnectionString("Default")));
        services.AddScoped<IApprovedApplicationStore, ApprovedApplicationStore>();
        services.AddScoped<IBlacklistSsnReader, BlacklistSsnReader>();
        services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<FundoTakeHomeDbContext>());

        return services;
    }
}
