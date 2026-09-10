using ErrorOr;

namespace FundoTakeHome.Api.Features.SubmitApplication.Application.Interfaces;

public interface IDecisionRule<in TInput>
{
    Task<ErrorOr<Success>> EvaluateAsync(TInput input, CancellationToken cancellationToken);
}
