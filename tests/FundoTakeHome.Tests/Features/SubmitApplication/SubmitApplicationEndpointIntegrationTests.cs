using FundoTakeHome.Api.Features.ApprovedApplicationDelivery.Application.Models;
using FundoTakeHome.Api.Features.SubmitApplication.Application.Models;
using FundoTakeHome.Api.Features.SubmitApplication.Domain.Common.Enums;
using FundoTakeHome.Api.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using FundoTakeHome.Tests.Features.SubmitApplication.Support;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace FundoTakeHome.Tests.Features.SubmitApplication;

public sealed class SubmitApplicationEndpointIntegrationTests
{
    [Fact]
    public async Task ShouldApproveNewCustomerAndPersistPendingOutboxMessage()
    {
        await using var factory = new SubmitApplicationApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/applications", CreateRequest());
        var document = await ReadResponseAsync(response);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();
        document.Errors.ShouldBeEmpty();
        document.Data.GetProperty("status").GetString().ShouldBe("approved");
        document.Data.GetProperty("customerId").GetGuid().ShouldNotBe(Guid.Empty);
        document.Data.GetProperty("applicationId").GetGuid().ShouldNotBe(Guid.Empty);

        await using var db = factory.CreateDbContext();
        (await db.Customers.CountAsync()).ShouldBe(1);
        (await db.Applications.CountAsync()).ShouldBe(1);
        (await db.OutboxMessages.CountAsync()).ShouldBe(1);
        (await db.OutboxMessages.SingleAsync()).Status.ShouldBe(OutboxStatusEnum.Pending);
    }

    [Fact]
    public async Task ShouldDenyNewYorkApplicationWithoutPersistence()
    {
        await using var factory = new SubmitApplicationApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/applications", CreateRequest() with { State = "NY" });
        var document = await ReadResponseAsync(response);

        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        document.Errors.ShouldContain(error => error.GetProperty("code").GetString() == "application.denied.state_ny" && error.GetProperty("field").GetString() == "state");

        await AssertDatabaseIsEmptyAsync(factory);
    }

    [Fact]
    public async Task ShouldDenyBlacklistedSsnWithoutPersistence()
    {
        await using var factory = new SubmitApplicationApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/applications", CreateRequest() with { Ssn = "000000000" });
        var document = await ReadResponseAsync(response);

        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        document.Errors.ShouldContain(error => error.GetProperty("code").GetString() == "application.denied.ssn_blacklisted" && error.GetProperty("field").GetString() == "ssn");

        await AssertDatabaseIsEmptyAsync(factory);
    }

    [Fact]
    public async Task ShouldReturnBothDenialReasonsWithoutPersistence()
    {
        await using var factory = new SubmitApplicationApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/applications", CreateRequest() with { State = "NY", Ssn = "000000000" });
        var document = await ReadResponseAsync(response);

        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        document.Errors.Select(error => error.GetProperty("code").GetString()).ShouldBe(["application.denied.state_ny", "application.denied.ssn_blacklisted"]);

        await AssertDatabaseIsEmptyAsync(factory);
    }

    [Fact]
    public async Task ShouldReturnFieldValidationErrorsWithoutPersistence()
    {
        await using var factory = new SubmitApplicationApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/applications", new SubmitApplicationRequest("", "", "", "XX", "", 0m, "invalid"));
        var document = await ReadResponseAsync(response);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        document.Data.ValueKind.ShouldBe(JsonValueKind.Null);
        document.Errors.Length.ShouldBe(7);
        AssertError(document.Errors, "customer.first_name_required", "FirstName", "First name is required.");
        AssertError(document.Errors, "customer.last_name_required", "LastName", "Last name is required.");
        AssertError(document.Errors, "customer.address_required", "Address", "Address is required.");
        AssertError(document.Errors, "customer.company_name_required", "CompanyName", "Company name is required.");
        AssertError(document.Errors, "state.invalid", "State", "State must be a valid USPS code.");
        AssertError(document.Errors, "ssn.invalid", "Ssn", "SSN must contain 9 digits.");
        AssertError(document.Errors, "requested_amount.invalid", "RequestedAmount", "Requested amount must be greater than 0 and less than 1000000.");

        await AssertDatabaseIsEmptyAsync(factory);
    }

    [Fact]
    public async Task ShouldUpdateReturningCustomerAndCreateUpdatedOutboxMessage()
    {
        await using var factory = new SubmitApplicationApiFactory();
        using var client = factory.CreateClient();

        var firstResponse = await client.PostAsJsonAsync("/api/applications", CreateRequest());
        var firstDocument = await ReadResponseAsync(firstResponse);
        var firstCustomerId = firstDocument.Data.GetProperty("customerId").GetGuid();
        var firstApplicationId = firstDocument.Data.GetProperty("applicationId").GetGuid();

        var secondResponse = await client.PostAsJsonAsync("/api/applications", CreateRequest() with
        {
            FirstName = "Updated",
            RequestedAmount = 2000m
        });
        var secondDocument = await ReadResponseAsync(secondResponse);

        firstResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        secondResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        secondDocument.Data.GetProperty("customerId").GetGuid().ShouldBe(firstCustomerId);
        secondDocument.Data.GetProperty("applicationId").GetGuid().ShouldBe(firstApplicationId);

        await using var db = factory.CreateDbContext();
        var customer = await db.Customers.SingleAsync();
        var application = await db.Applications.SingleAsync();
        customer.FirstName.ShouldBe("Updated");
        application.CustomerId.Value.ShouldBe(customer.Id.Value);
        application.RequestedAmount.Value.ShouldBe(2000m);
        (await db.Customers.CountAsync()).ShouldBe(1);
        (await db.Applications.CountAsync()).ShouldBe(1);

        var outboxMessages = await db.OutboxMessages.ToListAsync();
        outboxMessages.Count.ShouldBe(2);
        var operations = new List<EntityOperationEnum>();
        foreach (var message in outboxMessages)
        {
            OutboxEventEnvelope.TryParse(message.Payload, out var envelope).ShouldBeTrue();
            operations.Add(envelope!.Operation);
        }

        operations.ShouldContain(EntityOperationEnum.Created);
        operations.ShouldContain(EntityOperationEnum.Updated);
    }

    [Fact]
    public async Task ShouldRollbackNewCustomerWhenOutboxPersistenceFails()
    {
        await using var factory = new SubmitApplicationApiFactory();
        using var client = factory.CreateClient();
        await CreateOutboxInsertFailureTriggerAsync(factory);

        var response = await client.PostAsJsonAsync("/api/applications", CreateRequest());
        var document = await ReadResponseAsync(response);

        response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        AssertUnexpectedError(document);
        await AssertDatabaseIsEmptyAsync(factory);
    }

    [Fact]
    public async Task ShouldRollbackReturningCustomerWhenOutboxPersistenceFails()
    {
        await using var factory = new SubmitApplicationApiFactory();
        using var client = factory.CreateClient();

        var firstResponse = await client.PostAsJsonAsync("/api/applications", CreateRequest());
        firstResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        await CreateOutboxInsertFailureTriggerAsync(factory);

        var response = await client.PostAsJsonAsync("/api/applications", CreateRequest() with
        {
            FirstName = "Updated",
            RequestedAmount = 2000m
        });
        var document = await ReadResponseAsync(response);

        response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        AssertUnexpectedError(document);

        await using var db = factory.CreateDbContext();
        var customer = await db.Customers.SingleAsync();
        var application = await db.Applications.SingleAsync();
        customer.FirstName.ShouldBe("Jane");
        application.RequestedAmount.Value.ShouldBe(1000m);
        (await db.Customers.CountAsync()).ShouldBe(1);
        (await db.Applications.CountAsync()).ShouldBe(1);
        (await db.OutboxMessages.CountAsync()).ShouldBe(1);
    }

    private static SubmitApplicationRequest CreateRequest() => new("Jane", "Doe", "1 Main Street", "CA", "Fundo", 1000m, "123456789");

    private static async Task AssertDatabaseIsEmptyAsync(SubmitApplicationApiFactory factory)
    {
        await using var db = factory.CreateDbContext();
        (await db.Customers.CountAsync()).ShouldBe(0);
        (await db.Applications.CountAsync()).ShouldBe(0);
        (await db.OutboxMessages.CountAsync()).ShouldBe(0);
    }

    private static async Task<ApiResponseDocument> ReadResponseAsync(HttpResponseMessage response)
    {
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return new ApiResponseDocument(json.GetProperty("data"), json.GetProperty("errors").EnumerateArray().ToArray());
    }

    private static void AssertUnexpectedError(ApiResponseDocument document)
    {
        document.Data.ValueKind.ShouldBe(JsonValueKind.Null);
        document.Errors.Length.ShouldBe(1);
        AssertError(document.Errors, "system.unexpected_error", null, "An unexpected error occurred.");
    }

    private static async Task CreateOutboxInsertFailureTriggerAsync(SubmitApplicationApiFactory factory)
    {
        await using var db = factory.CreateDbContext();
        await db.Database.ExecuteSqlRawAsync("""
            CREATE TRIGGER FailOutboxInsert
            BEFORE INSERT ON OutboxMessages
            BEGIN
                SELECT RAISE(ABORT, 'forced test failure');
            END;
            """);
    }

    private static void AssertError(JsonElement[] errors, string code, string? field, string message)
    {
        errors.ShouldContain(error =>
            error.GetProperty("code").GetString() == code &&
            error.GetProperty("field").GetString() == field &&
            error.GetProperty("message").GetString() == message);
    }

    private sealed record ApiResponseDocument(JsonElement Data, JsonElement[] Errors);
}
