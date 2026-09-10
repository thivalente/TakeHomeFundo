using ErrorOr;

namespace FundoTakeHome.Backend.Features.SubmitApplication.Domain.Common.Errors;

public static class RequestedAmountErrors
{
    public static Error Invalid => Error.Validation("requested_amount.invalid", "Requested amount must be greater than 0 and less than 1000000.");
}
