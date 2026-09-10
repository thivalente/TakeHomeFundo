namespace FundoTakeHome.Backend.Features.SubmitApplication.Application.Interfaces;

public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}
