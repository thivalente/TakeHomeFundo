using ErrorOr;

namespace FundoTakeHome.Backend.Features.SubmitApplication.Domain.Common.Errors;

public static class UsStateErrors
{
    public static Error Invalid => Error.Validation("state.invalid", "State must be a valid USPS code.");
}
