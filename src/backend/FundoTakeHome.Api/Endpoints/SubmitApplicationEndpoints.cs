using FluentValidation;
using FundoTakeHome.Api.Common;
using FundoTakeHome.Api.Features.SubmitApplication.Application;
using FundoTakeHome.Api.Features.SubmitApplication.Application.Common.Errors;
using FundoTakeHome.Api.Features.SubmitApplication.Application.Models;
using FundoTakeHome.Api.Features.SubmitApplication.Application.Validators;

namespace FundoTakeHome.Api.Endpoints;

public static class SubmitApplicationEndpoints
{
    public static void MapSubmitApplicationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/applications", HandleAsync).WithName("SubmitApplication");
    }

    private static async Task<IResult> HandleAsync(SubmitApplicationRequest request, IValidator<SubmitApplicationRequest> validator, SubmitApplicationHandler handler, HttpResponse response, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);

        if (!validation.IsValid)
        {
            var errors = validation.Errors.Select(error => new ApiError(error.ErrorCode, error.PropertyName, error.ErrorMessage)).ToArray();
            return Results.BadRequest(new ApiResponse<object>(null, errors));
        }

        var result = await handler.HandleAsync(request, cancellationToken);

        if (result.IsError)
        {
            var errors = result.Errors.Select(error => new ApiError(error.Code, ApplicationDenialErrorMapping.GetField(error), error.Description)).ToArray();
            var statusCode = result.Errors.Any(ApplicationDenialErrorMapping.IsDenial) ? StatusCodes.Status422UnprocessableEntity : StatusCodes.Status400BadRequest;
            return Results.Json(new ApiResponse<object>(null, errors), statusCode: statusCode);
        }

        var responseData = new { result.Value.ApplicationId, result.Value.CustomerId, result.Value.Status };
        var data = new ApiResponse<object>(responseData, []);

        if (result.Value.Created)
        {
            response.Headers.Location = $"/api/applications/{result.Value.ApplicationId}";
            return Results.Json(data, statusCode: StatusCodes.Status201Created);
        }

        return Results.Ok(data);
    }
}
