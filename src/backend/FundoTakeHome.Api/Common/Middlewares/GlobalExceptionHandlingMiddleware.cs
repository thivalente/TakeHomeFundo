using System.Net;
using System.Text.Json;
using FundoTakeHome.Api.Common;

namespace FundoTakeHome.Api.Common.Middlewares;

public sealed class GlobalExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception exception) when (!httpContext.Response.HasStarted)
        {
            _logger.LogError(exception, "Unhandled exception while processing {RequestMethod} {RequestPath}", httpContext.Request.Method, httpContext.Request.Path);

            httpContext.Response.Clear();
            httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            httpContext.Response.ContentType = "application/json";

            var response = new ApiResponse<object?>(null, [new ApiError("system.unexpected_error", null, "An unexpected error occurred.")]);

            await httpContext.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
        }
    }
}

