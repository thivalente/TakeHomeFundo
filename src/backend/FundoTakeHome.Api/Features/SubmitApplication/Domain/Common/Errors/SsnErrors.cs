using ErrorOr;

namespace FundoTakeHome.Api.Features.SubmitApplication.Domain.Common.Errors;

public static class SsnErrors
{
    public static Error Invalid => Error.Validation("ssn.invalid", "SSN must contain 9 digits.");
}
