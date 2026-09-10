using ErrorOr;

namespace FundoTakeHome.Api.Features.SubmitApplication.Application.Interfaces;

public interface IDecisionRule<in TInput>
{
    ErrorOr<Success> Evaluate(TInput input);
}
