namespace FundoTakeHome.Api.Common;

public sealed record ApiError(string Code, string? Field, string Message);

public sealed record ApiResponse<T>(T? Data, IReadOnlyCollection<ApiError> Errors);

