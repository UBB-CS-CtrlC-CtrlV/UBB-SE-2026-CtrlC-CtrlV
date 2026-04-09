using BankApp.Contracts.Entities;
using ErrorOr;

namespace BankApp.Server.DataAccess.Interfaces;

/// <summary>
/// Defines data access operations for password reset tokens.
/// </summary>
public interface IPasswordResetTokenDataAccess
{
    /// <summary>
    /// Creates a new password reset token for the specified user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="tokenHash">The hashed token value.</param>
    /// <param name="expiresAt">The UTC expiration time of the token.</param>
    /// <returns>The newly created <see cref="PasswordResetToken"/>, or an error if the operation failed.</returns>
    ErrorOr<PasswordResetToken> Create(int userId, string tokenHash, DateTime expiresAt);

    /// <summary>
    /// Finds a password reset token by its hash.
    /// </summary>
    /// <param name="tokenHash">The hashed token value to search for.</param>
    /// <returns>The matching <see cref="PasswordResetToken"/>, or <see langword="null"/> if not found.</returns>
    PasswordResetToken? FindByToken(string tokenHash);

    /// <summary>
    /// Marks a password reset token as used.
    /// </summary>
    /// <param name="tokenId">The identifier of the token.</param>
    void MarkAsUsed(int tokenId);

    /// <summary>
    /// Deletes all expired password reset tokens.
    /// </summary>
    void DeleteExpired();
}