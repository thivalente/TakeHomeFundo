using ErrorOr;

namespace FundoTakeHome.Api.Features.SubmitApplication.Application.DecisionRules;

public interface IDecisionRuleEngine
{
    Task<IReadOnlyList<Error>> EvaluateAsync(DecisionRuleInput input, CancellationToken cancellationToken);
}
