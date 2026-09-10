using FundoTakeHome.Backend.Features.SubmitApplication.Application.Interfaces;

namespace FundoTakeHome.Backend.Infrastructure.Time;

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
