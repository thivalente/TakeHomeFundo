using ErrorOr;

namespace FundoTakeHome.Api.Common.Validation;

public sealed record PropertyValidationError(string PropertyName, Error Error);
