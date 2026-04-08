using BankApp.Contracts.Entities;

namespace BankApp.Server.Repositories.Interfaces;

/// <summary>
/// Defines repository operations for user profile management, sessions, OAuth links, and notification preferences.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Finds a user by their unique identifier.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>The matching <see cref="User"/>, or <see langword="null"/> if not found.</returns>
    User? FindById(int userId);

    /// <summary>
    /// Updates an existing user record.
    /// </summary>
    /// <param name="user">The user entity with updated values.</param>
    /// <returns><see langword="true"/> if the user was updated successfully; otherwise, <see langword="false"/>.</returns>
    bool UpdateUser(User user);

    /// <summary>
    /// Updates the password hash for the specified user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="newPasswordHash">The new hashed password.</param>
    /// <returns><see langword="true"/> if the password was updated; otherwise, <see langword="false"/>.</returns>
    bool UpdatePassword(int userId, string newPasswordHash);

    /// <summary>
    /// Gets all active sessions for the specified user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <returns>A list of active sessions.</returns>
    List<Session> GetActiveSessions(int userId);

    /// <summary>
    /// Revokes a single session by its identifier.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    void RevokeSession(int sessionId);

    /// <summary>
    /// Gets all OAuth provider links for the specified user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <returns>A list of OAuth links.</returns>
    List<OAuthLink> GetLinkedProviders(int userId);

    /// <summary>
    /// Creates a new OAuth link for the specified user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="provider">The OAuth provider name.</param>
    /// <param name="providerUserId">The user identifier issued by the provider.</param>
    /// <param name="email">The email address from the provider, or <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the link was created successfully; otherwise, <see langword="false"/>.</returns>
    bool SaveOAuthLink(int userId, string provider, string providerUserId, string? email);

    /// <summary>
    /// Deletes an OAuth link by its identifier.
    /// </summary>
    /// <param name="linkId">The OAuth link identifier.</param>
    void DeleteOAuthLink(int linkId);

    /// <summary>
    /// Gets all notification preferences for the specified user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <returns>A list of notification preferences.</returns>
    List<NotificationPreference> GetNotificationPreferences(int userId);

    /// <summary>
    /// Replaces all notification preferences for the specified user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="preferences">The updated list of notification preferences.</param>
    /// <returns><see langword="true"/> if the update succeeded; otherwise, <see langword="false"/>.</returns>
    bool UpdateNotificationPreferences(int userId, List<NotificationPreference> preferences);
}