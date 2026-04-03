using BankApp.Core.Enums;

namespace BankApp.Infrastructure.Repositories.Implementations;

using BankApp.Core.Entities;
using BankApp.Infrastructure.DataAccess.Interfaces;
using BankApp.Infrastructure.Repositories.Interfaces;

/// <summary>
/// Provides repository operations for user profile management, sessions, OAuth links, and notification preferences.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly IUserDataAccess userDataAccess;
    private readonly ISessionDataAccess sessionDataAccess;
    private readonly IOAuthLinkDataAccess oAuthLinkDataAccess;
    private readonly INotificationPreferenceDataAccess notificationPreferenceDataAccess;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserRepository"/> class.
    /// </summary>
    /// <param name="userDataAccess">The user data access component.</param>
    /// <param name="sessionDataAccess">The session data access component.</param>
    /// <param name="oAuthLinkDataAccess">The OAuth link data access component.</param>
    /// <param name="notificationPreferenceDataAccess">The notification preference data access component.</param>
    public UserRepository(IUserDataAccess userDataAccess, ISessionDataAccess sessionDataAccess, IOAuthLinkDataAccess oAuthLinkDataAccess,
        INotificationPreferenceDataAccess notificationPreferenceDataAccess)
    {
        this.userDataAccess = userDataAccess;
        this.sessionDataAccess = sessionDataAccess;
        this.notificationPreferenceDataAccess = notificationPreferenceDataAccess;
        this.oAuthLinkDataAccess = oAuthLinkDataAccess;
    }

    /// <inheritdoc />
    public User? FindById(int id)
    {
        return userDataAccess.FindById(id);
    }

    /// <inheritdoc />
    public bool UpdateUser(User user)
    {
        return userDataAccess.Update(user);
    }

    /// <inheritdoc />
    public bool UpdatePassword(int userId, string newPasswordHash)
    {
        return userDataAccess.UpdatePassword(userId, newPasswordHash);
    }

    /// <inheritdoc />
    public List<Session> GetActiveSessions(int userId)
    {
        return sessionDataAccess.FindByUserId(userId);
    }

    /// <inheritdoc />
    public void RevokeSession(int sessionId)
    {
        sessionDataAccess.Revoke(sessionId);
    }

    /// <inheritdoc />
    public List<OAuthLink> GetLinkedProviders(int userId)
    {
        return oAuthLinkDataAccess.FindByUserId(userId);
    }

    /// <inheritdoc />
    public bool SaveOAuthLink(int userId, string provider, string providerUserId, string? email)
    {
        return oAuthLinkDataAccess.Create(userId, provider, providerUserId, email);
    }

    /// <inheritdoc />
    public void DeleteOAuthLink(int linkId)
    {
        oAuthLinkDataAccess.Delete(linkId);
    }

    /// <inheritdoc />
    public List<NotificationPreference> GetNotificationPreferences(int userId)
    {
        return notificationPreferenceDataAccess.FindByUserId(userId);
    }

    /// <inheritdoc />
    public bool UpdateNotificationPreferences(int userId, List<NotificationPreference> preferences)
    {
        return notificationPreferenceDataAccess.Update(userId, preferences);
    }
}


