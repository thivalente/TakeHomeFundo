using ErrorOr;
using FundoTakeHome.Api.Features.SubmitApplication.Application.Interfaces;
using FundoTakeHome.Api.Features.SubmitApplication.Domain.Common.Errors;

namespace FundoTakeHome.Api.Features.SubmitApplication.Application.DecisionRules;

public sealed class StateIsNyRule : IDecisionRule<DecisionRuleInput>
{
    public Task<ErrorOr<Success>> EvaluateAsync(DecisionRuleInput input, CancellationToken cancellationToken)
    {
        if (!input.Customer.State.IsNewYork)
            return Task.FromResult<ErrorOr<Success>>(Result.Success);

        return Task.FromResult<ErrorOr<Success>>(ApplicationDenialErrors.StateIsNewYork);
    }
}
