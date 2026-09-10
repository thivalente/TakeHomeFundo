using FundoTakeHome.Api.Features.SubmitApplication.Application.Interfaces;
using FundoTakeHome.Api.Features.SubmitApplication.Domain.ValueObjects;
using FundoTakeHome.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FundoTakeHome.Api.Features.SubmitApplication.Infrastructure.Persistence.Blacklist;

public sealed class SqliteBlacklistSsnReader(FundoTakeHomeDbContext dbContext) : IBlacklistSsnReader
{
    public Task<bool> ExistsAsync(Ssn ssn, CancellationToken cancellationToken) =>
        dbContext.BlacklistedSsnRecords.AnyAsync(blacklistedSsn => blacklistedSsn.Ssn == ssn.Value, cancellationToken);
}
