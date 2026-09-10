using ErrorOr;
using FundoTakeHome.Backend.Features.SubmitApplication.Application.Interfaces;
using FundoTakeHome.Backend.Features.SubmitApplication.Domain.Common.Errors;

namespace FundoTakeHome.Backend.Features.SubmitApplication.Application.DecisionRules;

public sealed class BlacklistedSsnRule(IBlacklistSsnReader blacklistSsnReader) : IDecisionRule<DecisionRuleInput>
{
    public async Task<ErrorOr<Success>> EvaluateAsync(DecisionRuleInput input, CancellationToken cancellationToken)
    {
        if (!await blacklistSsnReader.ExistsAsync(input.Customer.Ssn, cancellationToken))
            return Result.Success;

        return ApplicationDenialErrors.SsnBlacklisted;
    }
}
