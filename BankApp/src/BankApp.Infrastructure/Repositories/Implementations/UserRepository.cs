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
    private readonly IUserDataAccess userDao;
    private readonly ISessionDataAccess sessionDao;
    private readonly IOAuthLinkDataAccess oAuthLinkDao;
    private readonly INotificationPreferenceDataAccess notificationPreferenceDao;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserRepository"/> class.
    /// </summary>
    /// <param name="userDao">The user data access component.</param>
    /// <param name="sessionDao">The session data access component.</param>
    /// <param name="oAuthLinkDao">The OAuth link data access component.</param>
    /// <param name="notificationPreferenceDao">The notification preference data access component.</param>
    public UserRepository(IUserDataAccess userDao, ISessionDataAccess sessionDao, IOAuthLinkDataAccess oAuthLinkDao,
        INotificationPreferenceDataAccess notificationPreferenceDao)
    {
        this.userDao = userDao;
        this.sessionDao = sessionDao;
        this.notificationPreferenceDao = notificationPreferenceDao;
        this.oAuthLinkDao = oAuthLinkDao;
    }

    /// <inheritdoc />
    public User? FindById(int id)
    {
        return userDao.FindById(id);
    }

    /// <inheritdoc />
    public bool UpdateUser(User user)
    {
        return userDao.Update(user);
    }

    /// <inheritdoc />
    public bool UpdatePassword(int userId, string newPasswordHash)
    {
        return userDao.UpdatePassword(userId, newPasswordHash);
    }

    /// <inheritdoc />
    public List<Session> GetActiveSessions(int userId)
    {
        return sessionDao.FindByUserId(userId);
    }

    /// <inheritdoc />
    public void RevokeSession(int sessionId)
    {
        sessionDao.Revoke(sessionId);
    }

    /// <inheritdoc />
    public List<OAuthLink> GetLinkedProviders(int userId)
    {
        return oAuthLinkDao.FindByUserId(userId);
    }

    /// <inheritdoc />
    public bool SaveOAuthLink(int userId, string provider, string providerUserId, string? email)
    {
        return oAuthLinkDao.Create(userId, provider, providerUserId, email);
    }

    /// <inheritdoc />
    public void DeleteOAuthLink(int linkId)
    {
        oAuthLinkDao.Delete(linkId);
    }

    /// <inheritdoc />
    public List<NotificationPreference> GetNotificationPreferences(int userId)
    {
        return notificationPreferenceDao.FindByUserId(userId);
    }

    /// <inheritdoc />
    public bool UpdateNotificationPreferences(int userId, List<NotificationPreference> prefs)
    {
        return notificationPreferenceDao.Update(userId, prefs);
    }
}


