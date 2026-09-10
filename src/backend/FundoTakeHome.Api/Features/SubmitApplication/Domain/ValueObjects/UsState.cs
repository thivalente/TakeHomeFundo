using ErrorOr;
using FundoTakeHome.Api.Features.SubmitApplication.Domain.Common.Errors;

namespace FundoTakeHome.Api.Features.SubmitApplication.Domain.ValueObjects;

public sealed class UsState
{
    public const string NewYorkCode = "NY";

    private static readonly HashSet<string> ValidStates = ["AL", "AK", "AZ", "AR", "CA", "CO", "CT", "DE", "FL", "GA", "HI", "ID", "IL", "IN", "IA", "KS", "KY", "LA", "ME", "MD", "MA", "MI", "MN", "MS", "MO", "MT", "NE", "NV", "NH", "NJ", "NM", "NY", "NC", "ND", "OH", "OK", "OR", "PA", "RI", "SC", "SD", "TN", "TX", "UT", "VT", "VA", "WA", "WV", "WI", "WY", "DC"];

    private UsState(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public bool IsNewYork => string.Equals(Value, NewYorkCode, StringComparison.Ordinal);

    public static ErrorOr<UsState> From(string? value)
    {
        var normalizedValue = value?.Trim().ToUpperInvariant();

        return normalizedValue is not null && ValidStates.Contains(normalizedValue) ? new UsState(normalizedValue) : UsStateErrors.Invalid;
    }
}
