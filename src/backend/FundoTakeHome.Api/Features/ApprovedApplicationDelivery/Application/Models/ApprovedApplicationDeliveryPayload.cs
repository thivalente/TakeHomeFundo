using FundoTakeHome.Api.Common.Extensions;
using FundoTakeHome.Api.Features.SubmitApplication.Domain.Common.Enums;

namespace FundoTakeHome.Api.Features.ApprovedApplicationDelivery.Application.Models;

public sealed record ApprovedApplicationDeliveryPayload(Guid EventId, EntityOperationEnum Operation, Guid CustomerId, string Ssn, string FirstName, string LastName, string Address, string State, string CompanyName, Guid ApplicationId, decimal RequestedAmount)
{
    public string ToLogString() =>
        $"Operation={Operation}, CustomerId={CustomerId}, ApplicationId={ApplicationId}, FirstName={FirstName}, State={State}, RequestedAmount={RequestedAmount}, Ssn={Ssn.Masked()}";
}
