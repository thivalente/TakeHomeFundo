using ErrorOr;

namespace FundoTakeHome.Backend.Features.SubmitApplication.Application.Interfaces;

public interface IDecisionRule<in TInput>
{
    Task<ErrorOr<Success>> EvaluateAsync(TInput input, CancellationToken cancellationToken);
}
