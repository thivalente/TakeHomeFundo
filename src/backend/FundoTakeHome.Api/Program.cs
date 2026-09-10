using FluentValidation;
using FundoTakeHome.Api.Infrastructure.Persistence;
using FundoTakeHome.Api.Features.SubmitApplication.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using FundoTakeHome.Api.Features.SubmitApplication.Endpoints;
using FundoTakeHome.Api.Features.SubmitApplication.Application;
using FundoTakeHome.Api.Features.SubmitApplication.Application.Interfaces;
using FundoTakeHome.Api.Infrastructure.Time;
using FundoTakeHome.Api.Common.Middlewares;

var builder = WebApplication.CreateBuilder(args);

Directory.CreateDirectory(Path.Combine(builder.Environment.ContentRootPath, "data"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddValidatorsFromAssemblyContaining<SubmitApplicationRequestValidator>();
builder.Services.AddDbContext<FundoTakeHomeDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddScoped<IApprovedApplicationStore, ApprovedApplicationStore>();
builder.Services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<FundoTakeHomeDbContext>());
builder.Services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
builder.Services.AddScoped<SubmitApplicationHandler>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<FundoTakeHomeDbContext>();
    dbContext.Database.EnsureCreated();
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
