namespace FundoTakeHome.Backend.Features.SubmitApplication.Application.Interfaces;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
