using System.Security.Claims;

namespace BankApp.Infrastructure.Services.Infrastructure.Interfaces;

public interface IJwtService
{
    string GenerateToken(int userId);
    ClaimsPrincipal? ValidateToken(string token);
    int? ExtractUserId(string token);
}
