using ErrorOr;

namespace FundoTakeHome.Api.Features.SubmitApplication.Domain.Common.Errors;

public static class CustomerErrors
{
    public static Error FirstNameRequired => Error.Validation("customer.first_name_required", "First name is required.");

    public static Error FirstNameTooLong => Error.Validation("customer.first_name_too_long", "First name is too long.");

    public static Error LastNameRequired => Error.Validation("customer.last_name_required", "Last name is required.");

    public static Error LastNameTooLong => Error.Validation("customer.last_name_too_long", "Last name is too long.");

    public static Error AddressRequired => Error.Validation("customer.address_required", "Address is required.");

    public static Error AddressTooLong => Error.Validation("customer.address_too_long", "Address is too long.");

    public static Error CompanyNameRequired => Error.Validation("customer.company_name_required", "Company name is required.");

    public static Error CompanyNameTooLong => Error.Validation("customer.company_name_too_long", "Company name is too long.");

    public static Error ApplicationNotFound => Error.Validation("customer.application_not_found", "The customer's application was not found.");

    public static Error ApplicationCustomerMismatch => Error.Validation("customer.application_customer_mismatch", "The application does not belong to the customer.");
}
