using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace InternshipManagement.Api.Middleware;

// Without this, a role rejection from [Authorize(Roles = "...")] (e.g. a Student token
// hitting a Company-only endpoint) is handled entirely by ASP.NET Core's authorization
// middleware, BEFORE any controller code runs - it writes a bare 401/403 with no body at
// all. That's a different code path from the Problem(statusCode: 403, ...) calls inside
// controllers (used for ownership checks, like "not your internship post"), which DO
// have a body. This wraps the default handler so both paths produce the same
// ProblemDetails shape - the whole point of Phase 12's "consistent error format" goal.
public class ProblemDetailsAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Forbidden)
        {
            var problemDetailsService = context.RequestServices.GetRequiredService<IProblemDetailsService>();
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await problemDetailsService.WriteAsync(new ProblemDetailsContext
            {
                HttpContext = context,
                ProblemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status403Forbidden,
                    Title = "Forbidden",
                    Detail = "You do not have the required role for this action.",
                    Instance = context.Request.GetEncodedPathAndQuery()
                }
            });
            return;
        }

        if (authorizeResult.Challenged)
        {
            var problemDetailsService = context.RequestServices.GetRequiredService<IProblemDetailsService>();
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await problemDetailsService.WriteAsync(new ProblemDetailsContext
            {
                HttpContext = context,
                ProblemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Unauthorized",
                    Detail = "A valid token is required for this endpoint.",
                    Instance = context.Request.GetEncodedPathAndQuery()
                }
            });
            return;
        }

        // Success (or a case this handler doesn't special-case) - fall back to the
        // framework's normal behavior.
        await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
    }
}
