using BankApp.Contracts.Entities;
using ErrorOr;

namespace BankApp.Server.Repositories.Interfaces;

/// <summary>
/// Defines repository operations for user profile management, sessions, OAuth links, and notification preferences.
/// </summary>
public interface IUserRepository
{
    /// <summary>Finds a user by their unique identifier.</summary>
    ErrorOr<User> FindById(int userId);

    /// <summary>Updates an existing user record.</summary>
    ErrorOr<Success> UpdateUser(User user);

    /// <summary>Updates the password hash for the specified user.</summary>
    ErrorOr<Success> UpdatePassword(int userId, string newPasswordHash);

    /// <summary>Gets all active sessions for the specified user.</summary>
    ErrorOr<List<Session>> GetActiveSessions(int userId);

    /// <summary>Revokes a single active session owned by the specified user.</summary>
    ErrorOr<Success> RevokeSession(int userId, int sessionId);

    /// <summary>Gets all OAuth provider links for the specified user.</summary>
    ErrorOr<List<OAuthLink>> GetLinkedProviders(int userId);

    /// <summary>Creates a new OAuth link for the specified user.</summary>
    ErrorOr<Success> SaveOAuthLink(int userId, string provider, string providerUserId, string? email);

    /// <summary>Deletes an OAuth link by its identifier.</summary>
    ErrorOr<Success> DeleteOAuthLink(int linkId);

    /// <summary>Gets all notification preferences for the specified user.</summary>
    ErrorOr<List<NotificationPreference>> GetNotificationPreferences(int userId);

    /// <summary>Replaces all notification preferences for the specified user.</summary>
    ErrorOr<Success> UpdateNotificationPreferences(int userId, List<NotificationPreference> preferences);
}
