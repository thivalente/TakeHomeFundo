using FundoTakeHome.Api.Features.ApprovedApplicationDelivery.Application.Models;

namespace FundoTakeHome.Api.Features.ApprovedApplicationDelivery.Application.Interfaces;

public interface IApprovedApplicationIntegration
{
    Task<IntegrationDeliveryResult> SendAsync(ApprovedApplicationDeliveryPayload delivery, CancellationToken cancellationToken);
}
