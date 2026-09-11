using FundoTakeHome.Api.Common.Interfaces;

namespace FundoTakeHome.Api.Infrastructure.Time;

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
