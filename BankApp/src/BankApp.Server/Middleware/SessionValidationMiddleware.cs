using BankApp.Contracts.Entities;
using BankApp.Server.Repositories.Interfaces;
using BankApp.Server.Services.Security;
using ErrorOr;

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
    /// <param name="logger">Logger for validation errors.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task Invoke(HttpContext context, IAuthRepository authRepository, IJwtService jwtService, ILogger<SessionValidationMiddleware> logger)
    {
        string? path = context.Request.Path.Value?.ToLower();

        // Public endpoints, no token needed
        if (IsPublicEndpoint(path))
        {
            await next(context);
            return;
        }

        string? authHeader = context.Request.Headers.Authorization.FirstOrDefault();

        // No token provided
        if (authHeader == null || !authHeader.StartsWith(BearerPrefix))
        {
            await RejectRequest(context, "No token provided.");
            return;
        }

        string token = authHeader[BearerPrefix.Length..];

        // Check if JWT valid
        ErrorOr<int> userIdResult = jwtService.ExtractUserId(token);
        if (userIdResult.IsError)
        {
            logger.LogWarning("Token validation failed [{Code}]: {Description}", userIdResult.FirstError.Code, userIdResult.FirstError.Description);
            await RejectRequest(context, "Invalid or expired token.");
            return;
        }

        // Check if session still active in the DB
        ErrorOr<Session> sessionResult = authRepository.FindSessionByToken(token);
        if (sessionResult.IsError)
        {
            logger.LogWarning("Session lookup failed [{Code}]: {Description}", sessionResult.FirstError.Code, sessionResult.FirstError.Description);
            await RejectRequest(context, "Invalid or expired token.");
            return;
        }

        // Store userId so controllers can use it
        context.Items["UserId"] = userIdResult.Value;

        await next(context);
    }

    private static bool IsPublicEndpoint(string? path)
    {
        return path is not null && Array.Exists(PublicEndpointPrefixes, path.Contains);
    }

    private static async Task RejectRequest(HttpContext context, string error)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { error });
    }
}