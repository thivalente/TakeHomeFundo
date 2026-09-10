using FundoTakeHome.Backend.Features.SubmitApplication.Domain.ValueObjects;

namespace FundoTakeHome.Backend.Features.SubmitApplication.Application.Interfaces;

public interface IBlacklistSsnReader
{
    Task<bool> ExistsAsync(Ssn ssn, CancellationToken cancellationToken);
}
