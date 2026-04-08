using System.Security.Claims;

namespace BankApp.Server.Services.Security;

/// <summary>
/// Defines operations for generating, validating, and extracting data from JSON Web Tokens.
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Generates a JWT for the specified user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <returns>The signed JWT string.</returns>
    string GenerateToken(int userId);

    /// <summary>
    /// Validates a JWT and returns the associated claims principal.
    /// </summary>
    /// <param name="token">The JWT string to validate.</param>
    /// <returns>The <see cref="ClaimsPrincipal"/> if valid; otherwise, <see langword="null"/>.</returns>
    ClaimsPrincipal? ValidateToken(string token);

    /// <summary>
    /// Extracts the user identifier from a JWT.
    /// </summary>
    /// <param name="token">The JWT string.</param>
    /// <returns>The user identifier if the token is valid; otherwise, <see langword="null"/>.</returns>
    int? ExtractUserId(string token);
}
