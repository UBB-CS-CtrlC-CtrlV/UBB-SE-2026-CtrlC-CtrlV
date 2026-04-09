using BankApp.Contracts.Entities;
using ErrorOr;

namespace BankApp.Server.Repositories.Interfaces;

/// <summary>
/// Defines repository operations for authentication, session management, and account security.
/// </summary>
public interface IAuthRepository
{
    /// <summary>Finds a user by their email address.</summary>
    ErrorOr<User> FindUserByEmail(string email);

    /// <summary>Creates a new user and initializes their default notification preferences.</summary>
    ErrorOr<Success> CreateUser(User user);

    /// <summary>Finds an OAuth link by its provider name and provider-specific user identifier.</summary>
    ErrorOr<OAuthLink> FindOAuthLink(string provider, string providerUserId);

    /// <summary>Creates a new OAuth link record.</summary>
    ErrorOr<Success> CreateOAuthLink(OAuthLink link);

    /// <summary>Creates a new session for the specified user.</summary>
    ErrorOr<Session> CreateSession(int userId, string token, string? deviceInfo, string? browser, string? ipAddress);

    /// <summary>Finds an active session by its token.</summary>
    ErrorOr<Session> FindSessionByToken(string token);

    /// <summary>Persists a password reset token.</summary>
    ErrorOr<Success> SavePasswordResetToken(PasswordResetToken token);

    /// <summary>Finds a password reset token by its hash.</summary>
    ErrorOr<PasswordResetToken> FindPasswordResetToken(string tokenHash);

    /// <summary>Marks a password reset token as used.</summary>
    ErrorOr<Success> MarkPasswordResetTokenAsUsed(int tokenId);

    /// <summary>Deletes all expired password reset tokens from the system.</summary>
    ErrorOr<Success> DeleteExpiredPasswordResetTokens();

    /// <summary>Revokes all active sessions for the specified user.</summary>
    ErrorOr<Success> InvalidateAllSessions(int userId);

    /// <summary>Finds a user by their unique identifier.</summary>
    ErrorOr<User> FindUserById(int id);

    /// <summary>Updates the password hash for the specified user.</summary>
    ErrorOr<Success> UpdatePassword(int userId, string newPasswordHash);

    /// <summary>Finds all active sessions for the specified user.</summary>
    ErrorOr<List<Session>> FindSessionsByUserId(int userId);

    /// <summary>Revokes the specified session, requiring the service layer to create a new one.</summary>
    ErrorOr<Success> UpdateSessionToken(int sessionId);

    /// <summary>Increments the failed login attempt counter for the specified user.</summary>
    ErrorOr<Success> IncrementFailedAttempts(int userId);

    /// <summary>Resets the failed login attempt counter to zero for the specified user.</summary>
    ErrorOr<Success> ResetFailedAttempts(int userId);

    /// <summary>Locks the specified user account until the given time.</summary>
    ErrorOr<Success> LockAccount(int userId, DateTime lockoutEnd);
}
