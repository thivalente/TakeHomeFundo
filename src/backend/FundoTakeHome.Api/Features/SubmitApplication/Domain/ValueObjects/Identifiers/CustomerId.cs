using ErrorOr;
using FundoTakeHome.Api.Features.SubmitApplication.Domain.Common.Errors;

namespace FundoTakeHome.Api.Features.SubmitApplication.Domain.ValueObjects.Identifiers;

public sealed class CustomerId
{
    private CustomerId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static ErrorOr<CustomerId> From(Guid value) => value == Guid.Empty ? IdentifierErrors.CustomerIdEmpty : new CustomerId(value);

    public static CustomerId New() => new CustomerId(Guid.CreateVersion7());
}
