using FundoTakeHome.Api.Features.SubmitApplication.Domain.Common.Enums;

namespace FundoTakeHome.Api.Features.ApprovedApplicationDelivery.Application.Models;

public sealed record ApprovedApplicationDeliveryPayload(Guid EventId, EntityOperationEnum Operation, Guid CustomerId, string Ssn, string FirstName, string LastName, string Address, string State, string CompanyName, Guid ApplicationId, decimal RequestedAmount);
