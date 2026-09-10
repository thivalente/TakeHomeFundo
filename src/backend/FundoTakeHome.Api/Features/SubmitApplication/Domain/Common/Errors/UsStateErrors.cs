using ErrorOr;

namespace FundoTakeHome.Api.Features.SubmitApplication.Domain.Common.Errors;

public static class UsStateErrors
{
    public static Error Invalid => Error.Validation("state.invalid", "State must be a valid USPS code.");
}
