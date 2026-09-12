using FundoTakeHome.Api.Common.Middlewares;
using FundoTakeHome.Api.Endpoints;
using FundoTakeHome.Api.Infrastructure.Persistence;
using FundoTakeHome.Api.Setup;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

Directory.CreateDirectory(Path.Combine(builder.Environment.ContentRootPath, "data"));

builder.Services.AddServicesConfig(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<FundoTakeHomeDbContext>();
    dbContext.Database.OpenConnection();
    dbContext.Database.ExecuteSqlRaw("PRAGMA journal_mode=DELETE;");
    dbContext.Database.CloseConnection();
    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
app.UseCors("Frontend");

app.MapSubmitApplicationEndpoints();

app.MapGet("/api/health", () => Results.Ok(new { status = "ok" })).WithName("Health");

app.Run();

public partial class Program;
