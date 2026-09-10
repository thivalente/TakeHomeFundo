using ErrorOr;

namespace FundoTakeHome.Backend.Features.SubmitApplication.Domain.Common.Errors;

public static class SsnErrors
{
    public static Error Invalid => Error.Validation("ssn.invalid", "SSN must contain 9 digits.");
}
