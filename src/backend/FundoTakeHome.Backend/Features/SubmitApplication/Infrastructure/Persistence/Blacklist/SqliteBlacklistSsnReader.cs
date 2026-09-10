using FundoTakeHome.Backend.Features.SubmitApplication.Application.Interfaces;
using FundoTakeHome.Backend.Features.SubmitApplication.Domain.ValueObjects;
using FundoTakeHome.Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FundoTakeHome.Backend.Features.SubmitApplication.Infrastructure.Persistence.Blacklist;

public sealed class SqliteBlacklistSsnReader(FundoTakeHomeDbContext dbContext) : IBlacklistSsnReader
{
    public Task<bool> ExistsAsync(Ssn ssn, CancellationToken cancellationToken) =>
        dbContext.BlacklistedSsnRecords.AnyAsync(blacklistedSsn => blacklistedSsn.Ssn == ssn.Value, cancellationToken);
}
