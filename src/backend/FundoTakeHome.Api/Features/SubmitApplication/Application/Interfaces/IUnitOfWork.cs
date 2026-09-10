namespace FundoTakeHome.Api.Features.SubmitApplication.Application.Interfaces;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
