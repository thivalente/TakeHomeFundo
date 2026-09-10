using ErrorOr;

namespace FundoTakeHome.Backend.Features.SubmitApplication.Domain.Common.Errors;

public static class LoanApplicationErrors
{
    public static Error CustomerIdRequired => Error.Validation("loan_application.customer_id_required", "Customer identifier is required.");
}
