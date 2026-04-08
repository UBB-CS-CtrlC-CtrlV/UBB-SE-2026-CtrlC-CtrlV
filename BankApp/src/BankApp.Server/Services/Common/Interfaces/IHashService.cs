namespace BankApp.Server.Services.Common.Interfaces;

/// <summary>
/// Defines operations for hashing and verifying values.
/// </summary>
public interface IHashService
{
    /// <summary>
    /// Computes a hash of the specified input.
    /// </summary>
    /// <param name="input">The plain-text value to hash.</param>
    /// <returns>The hashed representation of the input.</returns>
    string GetHash(string input);

    /// <summary>
    /// Verifies that a plain-text input matches a previously computed hash.
    /// </summary>
    /// <param name="input">The plain-text value to verify.</param>
    /// <param name="hash">The hash to verify against.</param>
    /// <returns><see langword="true"/> if the input matches the hash; otherwise, <see langword="false"/>.</returns>
    bool Verify(string input, string hash);
}
