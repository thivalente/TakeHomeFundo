namespace FundoTakeHome.Api.Features.SubmitApplication.Domain.Common.Validation;

public static class SubmitApplicationRules
{
    public const int FirstNameMaxLength = 100;
    public const int LastNameMaxLength = 100;
    public const int AddressMaxLength = 300;
    public const int CompanyNameMaxLength = 200;
    public const decimal RequestedAmountExclusiveMinimum = 0m;
    public const decimal RequestedAmountExclusiveMaximum = 1_000_000m;
}
