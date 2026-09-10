using FundoTakeHome.Backend.Features.SubmitApplication.Application.Interfaces;
using FundoTakeHome.Backend.Features.SubmitApplication.Domain.Entities;
using FundoTakeHome.Backend.Features.SubmitApplication.Domain.ValueObjects;
using FundoTakeHome.Backend.Features.SubmitApplication.Domain.ValueObjects.Identifiers;
using FundoTakeHome.Backend.Features.SubmitApplication.Infrastructure.Persistence.Outbox;
using FundoTakeHome.Backend.Features.SubmitApplication.Infrastructure.Persistence.Blacklist;
using Microsoft.EntityFrameworkCore;

namespace FundoTakeHome.Backend.Infrastructure.Persistence;

public sealed class FundoTakeHomeDbContext(DbContextOptions<FundoTakeHomeDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<LoanApplication> Applications => Set<LoanApplication>();

    public DbSet<BlacklistedSsnRecord> BlacklistedSsnRecords => Set<BlacklistedSsnRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.HasKey(message => message.Id);
            entity.Property(message => message.EventType).HasMaxLength(200).IsRequired();
            entity.Property(message => message.Payload).IsRequired();
            entity.Property(message => message.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(customer => customer.Id);
            entity.HasIndex(customer => customer.Ssn).IsUnique();
            entity.Property(customer => customer.Id).HasConversion(id => id.Value, value => CustomerId.From(value).Value!);
            entity.Property(customer => customer.Ssn).HasConversion(ssn => ssn.Value, value => Ssn.From(value).Value);
            entity.Property(customer => customer.State).HasConversion(state => state.Value, value => UsState.From(value).Value);
            entity.HasOne(customer => customer.Application).WithOne().HasForeignKey<LoanApplication>(application => application.CustomerId).IsRequired();
        });

        modelBuilder.Entity<LoanApplication>(entity =>
        {
            entity.HasKey(application => application.Id);
            entity.HasIndex(application => application.CustomerId).IsUnique();
            entity.Property(application => application.Id).HasConversion(id => id.Value, value => LoanApplicationId.From(value).Value!);
            entity.Property(application => application.CustomerId).HasConversion(id => id.Value, value => CustomerId.From(value).Value!);
            entity.Property(application => application.RequestedAmount).HasConversion(amount => amount.Value, value => RequestedAmount.From(value).Value);
            entity.Property(application => application.Status).HasConversion<string>();
        });

        modelBuilder.Entity<BlacklistedSsnRecord>(entity =>
        {
            entity.HasKey(blacklistedSsn => blacklistedSsn.Ssn);
            entity.Property(blacklistedSsn => blacklistedSsn.Ssn).HasMaxLength(9).IsRequired();
            entity.HasData(new BlacklistedSsnRecord { Ssn = "000000000" }, new BlacklistedSsnRecord { Ssn = "111111111" }, new BlacklistedSsnRecord { Ssn = "222222222" }, new BlacklistedSsnRecord { Ssn = "333333333" }, new BlacklistedSsnRecord { Ssn = "444444444" }, new BlacklistedSsnRecord { Ssn = "555555555" }, new BlacklistedSsnRecord { Ssn = "666666666" }, new BlacklistedSsnRecord { Ssn = "777777777" }, new BlacklistedSsnRecord { Ssn = "888888888" }, new BlacklistedSsnRecord { Ssn = "999999999" });
        });
    }
}
