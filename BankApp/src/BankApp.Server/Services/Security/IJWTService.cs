using System.Security.Claims;
using ErrorOr;

namespace BankApp.Server.Services.Security;

/// <summary>
/// Defines operations for generating, validating, and extracting data from JSON Web Tokens.
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Generates a signed JWT for the specified user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <returns>
    /// The signed JWT string on success,
    /// or a failure error with code <c>jwt.generate_failed</c> if the underlying cryptographic operation throws.
    /// </returns>
    ErrorOr<string> GenerateToken(int userId);

    /// <summary>
    /// Validates a JWT and returns the associated claims principal.
    /// </summary>
    /// <param name="token">The JWT string to validate.</param>
    /// <returns>
    /// The <see cref="ClaimsPrincipal"/> on success,
    /// or a validation error with code <c>token_expired</c> if the token has expired,
    /// or <c>token_invalid</c> if the signature is invalid or the token is malformed.
    /// </returns>
    ErrorOr<ClaimsPrincipal> ValidateToken(string token);

    /// <summary>
    /// Extracts the user identifier from a JWT.
    /// </summary>
    /// <param name="token">The JWT string.</param>
    /// <returns>
    /// The user identifier on success,
    /// or a validation error propagated from <see cref="ValidateToken"/> if the token is invalid or expired,
    /// or a validation error with code <c>token_missing_claim</c> if the token does not contain a valid user ID claim.
    /// </returns>
    ErrorOr<int> ExtractUserId(string token);
}
