using BankApp.Core.Entities;
namespace BankApp.Infrastructure.Repositories.Interfaces
{
    /// <summary>
    /// Defines repository operations for authentication, session management, and account security.
    /// </summary>
    public interface IAuthRepository
    {
        /// <summary>
        /// Finds a user by their email address.
        /// </summary>
        /// <param name="email">The email address to search for.</param>
        /// <returns>The matching <see cref="User"/>, or <see langword="null"/> if not found.</returns>
        User? FindUserByEmail(string email);

        /// <summary>
        /// Creates a new user and initializes their default notification preferences.
        /// </summary>
        /// <param name="user">The user entity to create.</param>
        /// <returns><see langword="true"/> if the user was created successfully; otherwise, <see langword="false"/>.</returns>
        bool CreateUser(User user);

        /// <summary>
        /// Finds an OAuth link by its provider name and provider-specific user identifier.
        /// </summary>
        /// <param name="provider">The OAuth provider name.</param>
        /// <param name="providerUserId">The user identifier issued by the provider.</param>
        /// <returns>The matching <see cref="OAuthLink"/>, or <see langword="null"/> if not found.</returns>
        OAuthLink? FindOAuthLink(string provider, string providerUserId);

        /// <summary>
        /// Creates a new OAuth link record.
        /// </summary>
        /// <param name="link">The OAuth link entity to create.</param>
        /// <returns><see langword="true"/> if the link was created successfully; otherwise, <see langword="false"/>.</returns>
        bool CreateOAuthLink(OAuthLink link);

        /// <summary>
        /// Creates a new session for the specified user.
        /// </summary>
        /// <param name="userId">The identifier of the user.</param>
        /// <param name="token">The unique session token.</param>
        /// <param name="deviceInfo">Optional device information.</param>
        /// <param name="browser">Optional browser name.</param>
        /// <param name="ip">Optional IP address.</param>
        /// <returns>The newly created <see cref="Session"/>.</returns>
        Session CreateSession(int userId, string token, string? deviceInfo, string? browser, string? ip);

        /// <summary>
        /// Finds an active session by its token.
        /// </summary>
        /// <param name="token">The session token to search for.</param>
        /// <returns>The matching <see cref="Session"/>, or <see langword="null"/> if not found or expired.</returns>
        Session? FindSessionByToken(string token);

        /// <summary>
        /// Persists a password reset token.
        /// </summary>
        /// <param name="token">The password reset token entity to save.</param>
        void SavePasswordResetToken(PasswordResetToken token);

        /// <summary>
        /// Finds a password reset token by its hash.
        /// </summary>
        /// <param name="tokenHash">The hashed token value.</param>
        /// <returns>The matching <see cref="PasswordResetToken"/>, or <see langword="null"/> if not found.</returns>
        PasswordResetToken? FindPasswordResetToken(string tokenHash);

        /// <summary>
        /// Marks a password reset token as used.
        /// </summary>
        /// <param name="tokenId">The identifier of the token.</param>
        void MarkPasswordResetTokenAsUsed(int tokenId);

        /// <summary>
        /// Revokes all active sessions for the specified user.
        /// </summary>
        /// <param name="userId">The identifier of the user.</param>
        void InvalidateAllSessions(int userId);

        /// <summary>
        /// Finds a user by their unique identifier.
        /// </summary>
        /// <param name="id">The user identifier.</param>
        /// <returns>The matching <see cref="User"/>, or <see langword="null"/> if not found.</returns>
        User? FindUserById(int id);

        /// <summary>
        /// Updates the password hash for the specified user.
        /// </summary>
        /// <param name="userId">The identifier of the user.</param>
        /// <param name="newPasswordHash">The new hashed password.</param>
        /// <returns><see langword="true"/> if the password was updated; otherwise, <see langword="false"/>.</returns>
        bool UpdatePassword(int userId, string newPasswordHash);

        /// <summary>
        /// Finds all active sessions for the specified user.
        /// </summary>
        /// <param name="userId">The identifier of the user.</param>
        /// <returns>A list of active sessions.</returns>
        List<Session> FindSessionsByUserId(int userId);

        /// <summary>
        /// Revokes the specified session token, requiring the service layer to create a new one.
        /// </summary>
        /// <param name="sessionId">The session identifier.</param>
        /// <returns><see langword="true"/> if the session was revoked successfully.</returns>
        bool UpdateSessionToken(int sessionId);

        /// <summary>
        /// Increments the failed login attempt counter for the specified user.
        /// </summary>
        /// <param name="userId">The identifier of the user.</param>
        void IncrementFailedAttempts(int userId);

        /// <summary>
        /// Resets the failed login attempt counter to zero for the specified user.
        /// </summary>
        /// <param name="userId">The identifier of the user.</param>
        void ResetFailedAttempts(int userId);

        /// <summary>
        /// Locks the specified user account until the given time.
        /// </summary>
        /// <param name="userId">The identifier of the user.</param>
        /// <param name="lockoutEnd">The UTC time when the lockout expires.</param>
        void LockAccount(int userId, DateTime lockoutEnd);
    }
}
