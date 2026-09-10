using ErrorOr;
using FundoTakeHome.Backend.Features.SubmitApplication.Domain.Common.Errors;

namespace FundoTakeHome.Backend.Features.SubmitApplication.Domain.ValueObjects.Identifiers;

public sealed class LoanApplicationId
{
    private LoanApplicationId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static ErrorOr<LoanApplicationId> From(Guid value) => value == Guid.Empty ? IdentifierErrors.ApplicationIdEmpty : new LoanApplicationId(value);

    public static LoanApplicationId New() => new LoanApplicationId(Guid.CreateVersion7());
}
