using FundoTakeHome.Api.Features.SubmitApplication.Domain.ValueObjects;

namespace FundoTakeHome.Api.Features.SubmitApplication.Application.Interfaces;

public interface IBlacklistSsnReader
{
    Task<bool> ExistsAsync(Ssn ssn, CancellationToken cancellationToken);
}
