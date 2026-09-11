using ErrorOr;
using FundoTakeHome.Api.Common.Extensions;
using FundoTakeHome.Api.Common.Validation;
using FundoTakeHome.Api.Features.SubmitApplication.Domain.Common.Errors;
using FundoTakeHome.Api.Features.SubmitApplication.Domain.Common.Validation;
using FundoTakeHome.Api.Features.SubmitApplication.Domain.ValueObjects;
using FundoTakeHome.Api.Features.SubmitApplication.Domain.ValueObjects.Identifiers;

namespace FundoTakeHome.Api.Features.SubmitApplication.Domain.Entities;

public sealed class Customer
{
    private Customer()
    { }

    private Customer(CustomerId id, Ssn ssn, string firstName, string lastName, string address, UsState state, string companyName)
    {
        Id = id;
        Ssn = ssn;
        Address = address.NormalizeWhitespace();
        CompanyName = companyName.NormalizeWhitespace();
        FirstName = firstName.NormalizeWhitespace();
        LastName = lastName.NormalizeWhitespace();
        State = state;
    }

    public string Address { get; private set; } = string.Empty;

    public string CompanyName { get; private set; } = string.Empty;

    public string FirstName { get; private set; } = string.Empty;

    public CustomerId Id { get; private set; } = null!;

    public LoanApplication? Application { get; private set; }

    public string LastName { get; private set; } = string.Empty;

    public Ssn Ssn { get; private set; } = null!;

    public UsState State { get; private set; } = null!;

    public static ErrorOr<Customer> Create(string ssn, string firstName, string lastName, string address, string state, string companyName)
    {
        var errors = Validate(ssn, firstName, lastName, address, state, companyName);

        if (errors.Count > 0)
            return errors.Select(error => error.Error).ToList();

        return new Customer(CustomerId.New(), Ssn.From(ssn).Value!, firstName, lastName, address, UsState.From(state).Value!, companyName);
    }

    public Customer AttachApplication(LoanApplication application)
    {
        Application = application;
        return this;
    }

    public ErrorOr<Success> Update(string firstName, string lastName, string address, string state, string companyName)
    {
        var errors = Validate(Ssn.Value, firstName, lastName, address, state, companyName);

        if (errors.Count > 0)
            return errors.Select(error => error.Error).ToList();

        Address = address.NormalizeWhitespace();
        CompanyName = companyName.NormalizeWhitespace();
        FirstName = firstName.NormalizeWhitespace();
        LastName = lastName.NormalizeWhitespace();
        State = UsState.From(state).Value!;

        return Result.Success;
    }

    public static IReadOnlyList<PropertyValidationError> Validate(string ssn, string firstName, string lastName, string address, string state, string companyName)
    {
        var errors = new List<PropertyValidationError>();

        ValidationHelpers.AddValueObjectErrors(errors, nameof(Ssn), Ssn.From(ssn));
        ValidationHelpers.AddValueObjectErrors(errors, nameof(State), UsState.From(state));
        ValidationHelpers.AddTextError(errors, nameof(FirstName), firstName, CustomerErrors.FirstNameRequired, CustomerErrors.FirstNameTooLong, SubmitApplicationRules.FirstNameMaxLength);
        ValidationHelpers.AddTextError(errors, nameof(LastName), lastName, CustomerErrors.LastNameRequired, CustomerErrors.LastNameTooLong, SubmitApplicationRules.LastNameMaxLength);
        ValidationHelpers.AddTextError(errors, nameof(Address), address, CustomerErrors.AddressRequired, CustomerErrors.AddressTooLong, SubmitApplicationRules.AddressMaxLength);
        ValidationHelpers.AddTextError(errors, nameof(CompanyName), companyName, CustomerErrors.CompanyNameRequired, CustomerErrors.CompanyNameTooLong, SubmitApplicationRules.CompanyNameMaxLength);

        return errors;
    }
}
