using ErrorOr;

namespace FundoTakeHome.Backend.Common.Validation;

public sealed record PropertyValidationError(string PropertyName, Error Error);
