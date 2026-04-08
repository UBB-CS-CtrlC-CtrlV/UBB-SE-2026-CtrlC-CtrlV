using BankApp.Server.Repositories.Interfaces;
using BankApp.Server.Services.Security;

namespace BankApp.Server.Middleware;

/// <summary>
/// Middleware that validates bearer tokens and active sessions on non-public endpoints.
/// </summary>
public class SessionValidationMiddleware
{
    private const string BearerPrefix = "Bearer ";
    private static readonly string[] PublicEndpointPrefixes = new[] { "/auth/", "/swagger", "/test/" };

    private readonly RequestDelegate next;

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionValidationMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the request pipeline.</param>
    public SessionValidationMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    /// <summary>
    /// Validates the authorization token and session, then invokes the next middleware.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="authRepository">The authentication repository used to verify sessions.</param>
    /// <param name="jwtService">The JWT service used to extract and validate tokens.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task Invoke(HttpContext context, IAuthRepository authRepository, IJwtService jwtService)
    {
        var path = context.Request.Path.Value?.ToLower();

        // Public endpoints, no token needed
        if (IsPublicEndpoint(path))
        {
            await next(context);
            return;
        }

        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

        // No token provided
        if (authHeader == null || !authHeader.StartsWith(BearerPrefix))
        {
            await RejectRequest(context, "No token provided.");
            return;
        }

        var token = authHeader.Substring(BearerPrefix.Length);

        // Check if JWT valid
        var userId = jwtService.ExtractUserId(token);
        if (userId == null)
        {
            await RejectRequest(context, "Invalid or expired token.");
            return;
        }

        // check if session still active in the DB
        var session = authRepository.FindSessionByToken(token);
        if (session == null)
        {
            await RejectRequest(context, "Session expired or revoked.");
            return;
        }

        // Store userId so controllers can use it
        context.Items["UserId"] = userId;

        await next(context);
    }

    private bool IsPublicEndpoint(string? path)
    {
        if (path == null)
        {
            return false;
        }

        return Array.Exists(PublicEndpointPrefixes, prefix => path.Contains(prefix));
    }

    private async Task RejectRequest(HttpContext context, string error)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { error });
    }
}