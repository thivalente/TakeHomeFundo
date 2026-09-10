using ErrorOr;

namespace FundoTakeHome.Backend.Features.SubmitApplication.Domain.Common.Errors;

public static class IdentifierErrors
{
    public static Error ApplicationIdEmpty => Error.Validation("application_id.empty", "Application identifier must not be empty.");

    public static Error CustomerIdEmpty => Error.Validation("customer_id.empty", "Customer identifier must not be empty.");

    public static Error OutboxMessageIdEmpty => Error.Validation("outbox_message_id.empty", "Outbox message identifier must not be empty.");
}
