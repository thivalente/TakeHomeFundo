using FundoTakeHome.Api.Features.SubmitApplication.Application.Interfaces;
using FundoTakeHome.Api.Features.SubmitApplication.Domain.Entities;
using FundoTakeHome.Api.Features.SubmitApplication.Domain.ValueObjects;
using FundoTakeHome.Api.Features.SubmitApplication.Domain.ValueObjects.Identifiers;
using FundoTakeHome.Api.Features.SubmitApplication.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;

namespace FundoTakeHome.Api.Infrastructure.Persistence;

public sealed class FundoTakeHomeDbContext(DbContextOptions<FundoTakeHomeDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<LoanApplication> Applications => Set<LoanApplication>();

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
    }
}
