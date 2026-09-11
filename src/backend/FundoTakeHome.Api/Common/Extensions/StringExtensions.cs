namespace FundoTakeHome.Api.Common.Extensions;

public static class StringExtensions
{
    public static string Masked(this string ssn)
    {
        if (string.IsNullOrWhiteSpace(ssn))
            return ssn;

        var digits = new string(ssn.Where(char.IsDigit).ToArray());
        return digits.Length >= 4 ? $"***-**-{digits[^4..]}" : "***-**-****";
    }

    public static string NormalizeWhitespace(this string value) => string.Join(' ', value.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
