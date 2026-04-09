using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using ErrorOr;

namespace BankApp.Server.Services.Security;

/// <summary>
/// Provides JWT generation, validation, and claim extraction using HMAC-SHA256.
/// </summary>
public class JwtService : IJwtService
{
    private readonly string secret;
    private const int TokenExpirationDays = 7;

    /// <summary>
    /// Initializes a new instance of the <see cref="JwtService"/> class.
    /// </summary>
    /// <param name="secret">The symmetric key used for signing tokens.</param>
    public JwtService(string secret)
    {
        this.secret = secret;
    }

    /// <inheritdoc />
    public ErrorOr<string> GenerateToken(int userId)
    {
        try
        {
            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.secret));
            SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            Claim[] claims = new[] { new Claim("userId", userId.ToString()) };
            JwtSecurityToken token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddDays(TokenExpirationDays),
                signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        catch (Exception ex)
        {
            return Error.Failure(code: "jwt.generate_failed", description: ex.Message);
        }
    }

    /// <inheritdoc />
    public ErrorOr<ClaimsPrincipal> ValidateToken(string token)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var handler = new JwtSecurityTokenHandler();

        try
        {
            ClaimsPrincipal principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                IssuerSigningKey = key
            }, out _);

            return principal;
        }
        catch (SecurityTokenExpiredException)
        {
            return Error.Validation(code: "token_expired", description: "The token has expired.");
        }
        catch (Exception)
        {
            return Error.Validation(code: "token_invalid", description: "The token signature is invalid or the token is malformed.");
        }
    }

    /// <inheritdoc />
    public ErrorOr<int> ExtractUserId(string token)
    {
        ErrorOr<ClaimsPrincipal> principalResult = ValidateToken(token);
        if (principalResult.IsError)
        {
            return principalResult.FirstError;
        }

        Claim? claim = principalResult.Value.FindFirst("userId");
         if (claim is not null && int.TryParse(claim.Value, out int userId))
        {
            return userId;
        }

        return Error.Validation(code: "token_missing_claim", description: "The token does not contain a valid user ID claim.");
    }
}