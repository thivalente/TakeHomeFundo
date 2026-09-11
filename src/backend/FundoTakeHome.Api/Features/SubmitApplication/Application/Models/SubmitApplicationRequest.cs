namespace FundoTakeHome.Api.Features.SubmitApplication.Application.Models;

public sealed record SubmitApplicationRequest(string FirstName, string LastName, string Address, string State, string CompanyName, decimal RequestedAmount, string Ssn);
