using FluentValidation;
using FundoTakeHome.Api.Infrastructure.Persistence;
using FundoTakeHome.Api.Features.SubmitApplication.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using FundoTakeHome.Api.Features.SubmitApplication.Application.Models;
using FundoTakeHome.Api.Features.SubmitApplication.Application.Validators;
using FundoTakeHome.Api.Features.SubmitApplication.Application.Interfaces;
using FundoTakeHome.Api.Infrastructure.Time;
using FundoTakeHome.Api.Common.Middlewares;
using FundoTakeHome.Api.Features.SubmitApplication.Application.DecisionRules;
using FundoTakeHome.Api.Features.SubmitApplication.Infrastructure.Persistence.Models;
using FundoTakeHome.Api.Features.ApprovedApplicationDelivery.Application.Interfaces;
using FundoTakeHome.Api.BackgroundServices;
using FundoTakeHome.Api.Features.ApprovedApplicationDelivery.Infrastructure.ExternalServices;
using FundoTakeHome.Api.Features.ApprovedApplicationDelivery.Infrastructure.Persistence;
using FundoTakeHome.Api.Common.Interfaces;
using FundoTakeHome.Api.Features.SubmitApplication.Application;
using FundoTakeHome.Api.Endpoints;
using FundoTakeHome.Api.Features.ApprovedApplicationDelivery.Application;

var builder = WebApplication.CreateBuilder(args);

Directory.CreateDirectory(Path.Combine(builder.Environment.ContentRootPath, "data"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddValidatorsFromAssemblyContaining<SubmitApplicationRequestValidator>();
builder.Services.AddDbContext<FundoTakeHomeDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddScoped<IApprovedApplicationStore, ApprovedApplicationStore>();
builder.Services.AddScoped<IBlacklistSsnReader, BlacklistSsnReader>();
builder.Services.AddScoped<IDecisionRule<DecisionRuleInput>, StateIsNyRule>();
builder.Services.AddScoped<IDecisionRule<DecisionRuleInput>, BlacklistedSsnRule>();
builder.Services.AddScoped<IDecisionRuleEngine, DecisionRuleEngine>();
builder.Services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<FundoTakeHomeDbContext>());
builder.Services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
builder.Services.AddScoped<SubmitApplicationHandler>();
builder.Services.AddScoped<IOutboxMessageStore, OutboxMessageStore>();
builder.Services.AddScoped<OutboxProcessor>();
builder.Services.AddHttpClient<IApprovedApplicationIntegration, ApprovedApplicationHttpClient>((serviceProvider, client) =>
{
    var baseUrl = serviceProvider.GetRequiredService<IConfiguration>()["ExternalService:BaseUrl"]
        ?? throw new InvalidOperationException("External service base URL is not configured.");

    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(5);
});
builder.Services.AddHostedService<OutboxBackgroundService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<FundoTakeHomeDbContext>();
    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.MapSubmitApplicationEndpoints();

app.MapGet("/api/health", () => Results.Ok(new { status = "ok" })).WithName("Health");

app.Run();
