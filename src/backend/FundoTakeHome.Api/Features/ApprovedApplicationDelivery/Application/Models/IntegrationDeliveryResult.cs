namespace FundoTakeHome.Api.Features.ApprovedApplicationDelivery.Application.Models;

public sealed record IntegrationDeliveryResult(bool Succeeded, string? Error)
{
    public static IntegrationDeliveryResult Success() => new(true, null);

    public static IntegrationDeliveryResult Failure(string error) => new(false, error);
}
