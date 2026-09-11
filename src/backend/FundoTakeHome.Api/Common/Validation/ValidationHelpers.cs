using ErrorOr;

namespace FundoTakeHome.Api.Common.Validation;

public static class ValidationHelpers
{
    public static void AddTextError(ICollection<PropertyValidationError> errors, string propertyName, string value, Error requiredError, Error tooLongError, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            errors.Add(new(propertyName, requiredError));
        else if (value.Length > maxLength)
            errors.Add(new(propertyName, tooLongError));
    }

    public static void AddValueObjectErrors<T>(ICollection<PropertyValidationError> errors, string propertyName, ErrorOr<T> result)
    {
        if (result.IsError)
            foreach (var error in result.Errors)
                errors.Add(new(propertyName, error));
    }
}
