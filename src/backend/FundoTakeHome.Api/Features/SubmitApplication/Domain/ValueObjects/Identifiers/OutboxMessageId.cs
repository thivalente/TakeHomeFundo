using ErrorOr;
using FundoTakeHome.Api.Features.SubmitApplication.Domain.Common.Errors;

namespace FundoTakeHome.Api.Features.SubmitApplication.Domain.ValueObjects.Identifiers;

public sealed class OutboxMessageId
{
    private OutboxMessageId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static ErrorOr<OutboxMessageId> From(Guid value) => value == Guid.Empty ? IdentifierErrors.OutboxMessageIdEmpty : new OutboxMessageId(value);

    public static ErrorOr<OutboxMessageId> New() => new OutboxMessageId(Guid.CreateVersion7());
}
