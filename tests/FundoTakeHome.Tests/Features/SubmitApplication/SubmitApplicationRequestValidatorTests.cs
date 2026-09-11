using FundoTakeHome.Api.Features.SubmitApplication.Application.Models;
using FundoTakeHome.Api.Features.SubmitApplication.Application.Validators;
using FundoTakeHome.Api.Features.SubmitApplication.Domain.Common.Validation;
using Shouldly;

namespace FundoTakeHome.Tests.Features.SubmitApplication;

public sealed class SubmitApplicationRequestValidatorTests
{
    [Fact]
    public async Task ShouldReturnAllErrors_WhenRequestIsInvalid()
    {
        var request = new SubmitApplicationRequest(" ", "", "", "XX", "", 0m, "invalid");

        var result = await new SubmitApplicationRequestValidator().ValidateAsync(request, CancellationToken.None);

        result.IsValid.ShouldBeFalse();
        result.Errors.Count.ShouldBe(7);
    }

    [Fact]
    public async Task ShouldRejectFieldsExceedingMaximumLength_WhenRequestIsTooLong()
    {
        var request = CreateValidRequest() with
        {
            FirstName = new string('a', SubmitApplicationRules.FirstNameMaxLength + 1),
            LastName = new string('a', SubmitApplicationRules.LastNameMaxLength + 1),
            Address = new string('a', SubmitApplicationRules.AddressMaxLength + 1),
            CompanyName = new string('a', SubmitApplicationRules.CompanyNameMaxLength + 1)
        };

        var result = await new SubmitApplicationRequestValidator().ValidateAsync(request, CancellationToken.None);

        result.IsValid.ShouldBeFalse();
        result.Errors.Select(error => error.ErrorCode).ShouldContain("customer.first_name_too_long");
        result.Errors.Select(error => error.ErrorCode).ShouldContain("customer.last_name_too_long");
        result.Errors.Select(error => error.ErrorCode).ShouldContain("customer.address_too_long");
        result.Errors.Select(error => error.ErrorCode).ShouldContain("customer.company_name_too_long");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1_000_000)]
    public async Task ShouldRejectRequestedAmountOutsideExclusiveRange_WhenAmountIsInvalid(decimal amount)
    {
        var result = await new SubmitApplicationRequestValidator().ValidateAsync(CreateValidRequest() with { RequestedAmount = amount }, CancellationToken.None);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(error => error.PropertyName == nameof(SubmitApplicationRequest.RequestedAmount));
    }

    [Fact]
    public async Task ShouldRejectInvalidStateAndSsn_WhenValuesAreInvalid()
    {
        var result = await new SubmitApplicationRequestValidator().ValidateAsync(CreateValidRequest() with { State = "XX", Ssn = "invalid" }, CancellationToken.None);

        result.IsValid.ShouldBeFalse();
        result.Errors.Select(error => error.ErrorCode).ShouldContain("state.invalid");
        result.Errors.Select(error => error.ErrorCode).ShouldContain("ssn.invalid");
    }

    [Fact]
    public async Task ShouldAcceptFormattedSsnAndValidState_WhenRequestIsValid()
    {
        var result = await new SubmitApplicationRequestValidator().ValidateAsync(CreateValidRequest() with { State = " ca ", Ssn = "123-45-6789" }, CancellationToken.None);

        result.IsValid.ShouldBeTrue();
    }

    private static SubmitApplicationRequest CreateValidRequest() => new("Jane", "Doe", "1 Main Street", "CA", "Fundo", 100m, "123456789");
}
