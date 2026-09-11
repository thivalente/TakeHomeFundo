using ErrorOr;

namespace FundoTakeHome.Api.Features.SubmitApplication.Domain.Common.Errors;

public static class ApplicationDenialErrors
{
    public const string CategoryKey = "category";
    public const string CategoryValue = "application_denial";
    public const string FieldKey = "field";

    public static Error SsnBlacklisted => Create("application.denied.ssn_blacklisted", "The provided SSN is not eligible.", "ssn");

    public static Error StateIsNewYork => Create("application.denied.state_ny", "Applications from New York are not eligible.", "state");

    public static string? GetField(Error error) => error.Metadata is not null && error.Metadata.TryGetValue(FieldKey, out var field) ? field?.ToString() : null;

    public static bool IsDenial(Error error) => error.Metadata is not null && error.Metadata.TryGetValue(CategoryKey, out var category) && string.Equals(category?.ToString(), CategoryValue, StringComparison.Ordinal);

    private static Error Create(string code, string description, string field) => Error.Validation(code, description, new Dictionary<string, object> { [CategoryKey] = CategoryValue, [FieldKey] = field });
}
