namespace FinanceOne.Api.Common;

public sealed class Response<TResult>
{
    // HTTP status code. Null when the request succeeded.
    public int? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }
    public TResult? Result { get; init; }

    public bool IsSuccess => ErrorCode is null;

    public static Response<TResult> Success(TResult result) =>
        new() { Result = result };

    public static Response<TResult> Failure(int errorCode, string errorMessage) =>
        new() { ErrorCode = errorCode, ErrorMessage = errorMessage };
}
