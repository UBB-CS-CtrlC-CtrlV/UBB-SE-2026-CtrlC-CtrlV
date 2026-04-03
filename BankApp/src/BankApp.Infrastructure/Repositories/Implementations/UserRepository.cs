using BankApp.Core.Enums;

namespace BankApp.Infrastructure.Repositories.Implementations;

using BankApp.Core.Entities;
using BankApp.Infrastructure.DataAccess.Interfaces;
using BankApp.Infrastructure.Repositories.Interfaces;
public class UserRepository : IUserRepository
{
    private readonly IUserDataAccess userDao;
    private readonly ISessionDataAccess sessionDao;
    private readonly IOAuthLinkDataAccess oAuthLinkDao;
    private readonly INotificationPreferenceDataAccess notificationPreferenceDao;

    public UserRepository(IUserDataAccess userDao, ISessionDataAccess sessionDao, IOAuthLinkDataAccess oAuthLinkDao,
        INotificationPreferenceDataAccess notificationPreferenceDao)
    {
        this.userDao = userDao;
        this.sessionDao = sessionDao;
        this.notificationPreferenceDao = notificationPreferenceDao;
        this.oAuthLinkDao = oAuthLinkDao;
    }

    public User? FindById(int id)
    {
        return userDao.FindById(id);
    }

    public bool UpdateUser(User user)
    {
        return userDao.Update(user);
    }

    public bool UpdatePassword(int userId, string newPasswordHash)
    {
        return userDao.UpdatePassword(userId, newPasswordHash);
    }

    public List<Session> GetActiveSessions(int userId)
    {
        return sessionDao.FindByUserId(userId);
    }

    public void RevokeSession(int sessionId)
    {
        sessionDao.Revoke(sessionId);
    }

    public List<OAuthLink> GetLinkedProviders(int userId)
    {
        return oAuthLinkDao.FindByUserId(userId);
    }

    public bool SaveOAuthLink(int userId, string provider, string providerUserId, string? email)
    {
        return oAuthLinkDao.Create(userId, provider, providerUserId, email);
    }

    public void DeleteOAuthLink(int linkId)
    {
        oAuthLinkDao.Delete(linkId);
    }

    public List<NotificationPreference> GetNotificationPreferences(int userId)
    {
        return notificationPreferenceDao.FindByUserId(userId);
    }

    public bool UpdateNotificationPreferences(int userId, List<NotificationPreference> prefs)
    {
        return notificationPreferenceDao.Update(userId, prefs);
    }
}


