using ErrorOr;

namespace FundoTakeHome.Api.Features.SubmitApplication.Application.Common.Errors;

public static class ApplicationDenialErrorMapping
{
    private const string CategoryKey = "category";
    private const string CategoryValue = "application_denial";
    private const string FieldKey = "field";

    public static string? GetField(Error error)
    {
        return error.Metadata is not null &&
               error.Metadata.TryGetValue(FieldKey, out var field)
            ? field?.ToString()
            : null;
    }

    public static bool IsDenial(Error error)
    {
        return error.Metadata is not null &&
               error.Metadata.TryGetValue(CategoryKey, out var category) &&
               string.Equals(category?.ToString(), CategoryValue, StringComparison.Ordinal);
    }
}
