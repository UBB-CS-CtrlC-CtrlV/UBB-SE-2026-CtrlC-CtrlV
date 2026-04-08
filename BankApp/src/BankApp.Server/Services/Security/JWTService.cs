using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using BankApp.Server.Services.Security;
using Microsoft.IdentityModel.Tokens;

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
    public string GenerateToken(int userId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("userId", userId.ToString())
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddDays(TokenExpirationDays),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <inheritdoc />
    public ClaimsPrincipal? ValidateToken(string token)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var handler = new JwtSecurityTokenHandler();

        try
        {
            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                IssuerSigningKey = key
            }, out _);

            return principal;
        }
        catch
        {
            return null;
        }
    }

    /// <inheritdoc />
    public int? ExtractUserId(string token)
    {
        var principal = ValidateToken(token);
        var claim = principal?.FindFirst("userId");

        if (claim != null && int.TryParse(claim.Value, out var userId))
        {
            return userId;
        }

        return null;
    }
}