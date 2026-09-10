using FundoTakeHome.Api.Features.SubmitApplication.Application.Interfaces;

namespace FundoTakeHome.Api.Infrastructure.Time;

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
