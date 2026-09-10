using ErrorOr;

namespace FundoTakeHome.Api.Features.SubmitApplication.Application.Interfaces;

public interface IDecisionRuleEngine
{
    Task<IReadOnlyList<Error>> EvaluateAsync(DecisionRuleInput input, CancellationToken cancellationToken);
}
