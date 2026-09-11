var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/customers", (ApprovedApplicationPayload payload, ILogger<Program> logger, HttpRequest request) =>
    ReceiveCustomer(payload, null, logger, request));

app.MapPut("/customers/{customerId}", (string customerId, ApprovedApplicationPayload payload, ILogger<Program> logger, HttpRequest request) =>
    ReceiveCustomer(payload, customerId, logger, request));

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();

static IResult ReceiveCustomer(ApprovedApplicationPayload payload, string? routeCustomerId, ILogger<Program> logger, HttpRequest request)
{
    var operation = routeCustomerId is null ? "Created" : "Updated";
    var customerId = routeCustomerId ?? payload.CustomerId.ToString();

    logger.LogInformation("Mock received request. Operation={Operation}, Method={Method}, {Customer}", request.Method, operation, payload.ToLogString());

    return Results.Ok(new { status = "received" });
}

public sealed record ApprovedApplicationPayload(Guid EventId, Guid CustomerId, string Ssn, string FirstName, string LastName, string Address, string State, string CompanyName, Guid ApplicationId, decimal RequestedAmount)
{
    public string ToLogString() =>
        $"CustomerId={CustomerId}, ApplicationId={ApplicationId}, FirstName={FirstName}, LastName={LastName}, State={State}, RequestedAmount={RequestedAmount}, Ssn={MaskSsn(Ssn)}";

    private static string MaskSsn(string ssn)
    {
        if (string.IsNullOrWhiteSpace(ssn))
            return ssn;

        var digits = new string(ssn.Where(char.IsDigit).ToArray());
        return digits.Length >= 4 ? $"***-**-{digits[^4..]}" : "***-**-****";
    }
}
