namespace FundoTakeHome.Api.Features.SubmitApplication.Application.Interfaces;

public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}
