namespace FundoTakeHome.Backend.Features.SubmitApplication.Application;

public sealed record SubmitApplicationResult(Guid ApplicationId, Guid CustomerId, string Status, bool Created);
