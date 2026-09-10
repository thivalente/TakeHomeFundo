using FluentValidation;
using FluentValidation.Results;
using FundoTakeHome.Api.Features.SubmitApplication.Domain.Entities;

namespace FundoTakeHome.Api.Features.SubmitApplication.Application;

public sealed class SubmitApplicationRequestValidator : AbstractValidator<SubmitApplicationRequest>
{
    public SubmitApplicationRequestValidator()
    {
        RuleFor(request => request).Custom((request, context) =>
        {
            foreach (var validation in Customer.Validate(request.Ssn, request.FirstName, request.LastName, request.Address, request.State, request.CompanyName))
                context.AddFailure(new ValidationFailure(validation.PropertyName, validation.Error.Description) { ErrorCode = validation.Error.Code });

            foreach (var validation in LoanApplication.Validate(null, request.RequestedAmount).Where(validation => validation.PropertyName == nameof(LoanApplication.RequestedAmount)))
                context.AddFailure(new ValidationFailure(validation.PropertyName, validation.Error.Description) { ErrorCode = validation.Error.Code });
        });
    }
}
