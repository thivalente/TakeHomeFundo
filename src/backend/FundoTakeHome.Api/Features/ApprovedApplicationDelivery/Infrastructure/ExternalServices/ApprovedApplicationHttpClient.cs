using FundoTakeHome.Api.Features.ApprovedApplicationDelivery.Application.Interfaces;
using FundoTakeHome.Api.Features.ApprovedApplicationDelivery.Application.Models;
using FundoTakeHome.Api.Features.SubmitApplication.Domain.Common.Enums;
using System.Net;
using System.Text.Json;

namespace FundoTakeHome.Api.Features.ApprovedApplicationDelivery.Infrastructure.ExternalServices;

public sealed class ApprovedApplicationHttpClient(HttpClient httpClient, ILogger<ApprovedApplicationHttpClient> logger) : IApprovedApplicationIntegration
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IntegrationDeliveryResult> SendAsync(ApprovedApplicationDeliveryPayload delivery, CancellationToken cancellationToken)
    {
        using var response = delivery.Operation == EntityOperationEnum.Created
            ? await httpClient.PostAsJsonAsync("customers", delivery, JsonOptions, cancellationToken)
            : await httpClient.PutAsJsonAsync($"customers/{delivery.CustomerId}", delivery, JsonOptions, cancellationToken);

        if (response.StatusCode == HttpStatusCode.OK)
            return IntegrationDeliveryResult.Success();

        logger.LogWarning("External integration returned HTTP {StatusCode} for event {EventId}.", (int)response.StatusCode, delivery.EventId);
        return IntegrationDeliveryResult.Failure($"External integration returned HTTP {(int)response.StatusCode}.");
    }
}
