using ErrorOr;

namespace FundoTakeHome.Backend.Features.SubmitApplication.Application.Interfaces;

public interface IDecisionRuleEngine
{
    Task<IReadOnlyList<Error>> EvaluateAsync(DecisionRuleInput input, CancellationToken cancellationToken);
}
