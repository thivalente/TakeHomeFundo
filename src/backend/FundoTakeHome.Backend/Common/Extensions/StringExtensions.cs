namespace FundoTakeHome.Backend.Common.Extensions;

public static class StringExtensions
{
    public static string NormalizeWhitespace(this string value) => string.Join(' ', value.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
