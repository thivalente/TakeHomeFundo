namespace FundoTakeHome.Api.Features.SubmitApplication.Application.Models;

public sealed record SubmitApplicationResult(Guid ApplicationId, Guid CustomerId, string Status, bool Created);
