using ErrorOr;
using FundoTakeHome.Api.Features.SubmitApplication.Application.Interfaces;

namespace FundoTakeHome.Api.Features.SubmitApplication.Application;

public sealed class DecisionRuleEngine(IEnumerable<IDecisionRule<DecisionRuleInput>> decisionRules) : IDecisionRuleEngine
{
    public async Task<IReadOnlyList<Error>> EvaluateAsync(DecisionRuleInput input, CancellationToken cancellationToken)
    {
        var errors = new List<Error>();

        foreach (var decisionRule in decisionRules)
        {
            var result = await decisionRule.EvaluateAsync(input, cancellationToken);

            if (result.IsError)
                errors.AddRange(result.Errors);
        }

        return errors;
    }
}
