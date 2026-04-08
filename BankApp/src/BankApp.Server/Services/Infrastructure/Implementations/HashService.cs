using BankApp.Server.Services.Infrastructure.Interfaces;

namespace BankApp.Server.Services.Infrastructure.Implementations;

/// <summary>
/// Provides BCrypt-based password hashing and verification.
/// </summary>
public class HashService : IHashService
{
    /// <inheritdoc />
    public string GetHash(string input)
    {
        return BCrypt.Net.BCrypt.HashPassword(input);
    }

    /// <inheritdoc />
    public bool Verify(string input, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(input, hash);
    }
}