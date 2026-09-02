using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FinanceOne.Api.Common;

// Catches unexpected failures (DB unavailable, bugs) centrally. Expected failures
// (not found, conflict, invalid state) never throw — they're returned via
// Response<T>.Failure and never reach this handler.
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception");
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(
            new ProblemDetails { Status = 500, Title = "An unexpected error occurred." },
            cancellationToken);
        return true;
    }
}
