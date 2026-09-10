using System.Text.RegularExpressions;
using ErrorOr;
using FundoTakeHome.Api.Features.SubmitApplication.Domain.Common.Errors;

namespace FundoTakeHome.Api.Features.SubmitApplication.Domain.ValueObjects;

public sealed class Ssn
{
    private Ssn(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static ErrorOr<Ssn> From(string? value)
    {
        var trimmedValue = value?.Trim();

        if (trimmedValue is null || !Regex.IsMatch(trimmedValue, "^(\\d{9}|\\d{3}-\\d{2}-\\d{4})$", RegexOptions.CultureInvariant))
            return SsnErrors.Invalid;

        return new Ssn(trimmedValue.Replace("-", string.Empty, StringComparison.Ordinal));
    }

    public override string ToString() => "***-**-****";
}
