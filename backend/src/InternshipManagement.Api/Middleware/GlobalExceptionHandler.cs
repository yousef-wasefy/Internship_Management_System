using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace InternshipManagement.Api.Middleware;

// Catches anything that reaches here uncaught - a real bug, not an expected business
// outcome (those are all handled in the controllers via Problem(...) already). Logs the
// real exception server-side, but the client only ever sees a generic message: leaking
// exception details/stack traces in a response is an information-disclosure risk, not
// just an ugly response.
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unhandled exception for {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred.",
            Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1"
            // Deliberately no Detail here - never echo exception.Message to the client.
        }, cancellationToken);

        return true; // "handled" - stop ASP.NET Core from doing anything further with it
    }
}
