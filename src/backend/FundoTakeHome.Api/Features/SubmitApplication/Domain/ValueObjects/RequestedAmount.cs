using ErrorOr;
using FundoTakeHome.Api.Features.SubmitApplication.Domain.Common.Errors;

namespace FundoTakeHome.Api.Features.SubmitApplication.Domain.ValueObjects;

public sealed class RequestedAmount
{
    private RequestedAmount(decimal value)
    {
        Value = value;
    }

    public decimal Value { get; }

    public static ErrorOr<RequestedAmount> From(decimal value) => value <= 0 || value >= 1_000_000m ? RequestedAmountErrors.Invalid : new RequestedAmount(value);
}
