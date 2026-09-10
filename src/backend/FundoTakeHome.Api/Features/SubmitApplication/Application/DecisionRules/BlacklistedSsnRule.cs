using ErrorOr;
using FundoTakeHome.Api.Features.SubmitApplication.Application.Interfaces;
using FundoTakeHome.Api.Features.SubmitApplication.Domain.Common.Errors;

namespace FundoTakeHome.Api.Features.SubmitApplication.Application.DecisionRules;

public sealed class BlacklistedSsnRule(IBlacklistSsnReader blacklistSsnReader) : IDecisionRule<DecisionRuleInput>
{
    public async Task<ErrorOr<Success>> EvaluateAsync(DecisionRuleInput input, CancellationToken cancellationToken)
    {
        if (!await blacklistSsnReader.ExistsAsync(input.Customer.Ssn, cancellationToken))
            return Result.Success;

        return ApplicationDenialErrors.SsnBlacklisted;
    }
}
