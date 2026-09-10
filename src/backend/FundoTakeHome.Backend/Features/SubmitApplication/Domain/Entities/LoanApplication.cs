using ErrorOr;
using FundoTakeHome.Backend.Common.Validation;
using FundoTakeHome.Backend.Features.SubmitApplication.Domain.Common.Errors;
using FundoTakeHome.Backend.Features.SubmitApplication.Domain.Enums;
using FundoTakeHome.Backend.Features.SubmitApplication.Domain.ValueObjects;
using FundoTakeHome.Backend.Features.SubmitApplication.Domain.ValueObjects.Identifiers;

namespace FundoTakeHome.Backend.Features.SubmitApplication.Domain.Entities;

public sealed class LoanApplication
{
    private LoanApplication()
    { }

    private LoanApplication(LoanApplicationId id, CustomerId customerId, RequestedAmount requestedAmount)
    {
        Id = id;
        CustomerId = customerId;
        RequestedAmount = requestedAmount;
        Status = ApplicationStatusEnum.Approved;
    }

    public CustomerId CustomerId { get; private set; } = null!;

    public LoanApplicationId Id { get; private set; } = null!;

    public RequestedAmount RequestedAmount { get; private set; } = null!;

    public ApplicationStatusEnum Status { get; private set; }

    public static ErrorOr<LoanApplication> Create(CustomerId? customerId, decimal requestedAmount)
    {
        var errors = Validate(customerId, requestedAmount);

        if (errors.Count > 0)
            return errors.Select(error => error.Error).ToList();

        return new LoanApplication(LoanApplicationId.New(), customerId!, RequestedAmount.From(requestedAmount).Value!);
    }

    public ErrorOr<Success> Update(decimal requestedAmount)
    {
        var requestedAmountResult = RequestedAmount.From(requestedAmount);

        if (requestedAmountResult.IsError)
            return requestedAmountResult.Errors;

        RequestedAmount = requestedAmountResult.Value;
        Status = ApplicationStatusEnum.Approved;
        return Result.Success;
    }

    public static IReadOnlyList<PropertyValidationError> Validate(CustomerId? customerId, decimal requestedAmount)
    {
        var errors = new List<PropertyValidationError>();

        if (customerId is null)
            errors.Add(new(nameof(CustomerId), LoanApplicationErrors.CustomerIdRequired));

        ValidationHelpers.AddValueObjectErrors(errors, nameof(RequestedAmount), RequestedAmount.From(requestedAmount));

        return errors;
    }
}
