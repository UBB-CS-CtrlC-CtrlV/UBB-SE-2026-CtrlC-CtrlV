using BankApp.Core.Enums;

namespace BankApp.Infrastructure.Repositories.Implementations;

using BankApp.Core.Entities;
using BankApp.Infrastructure.DataAccess.Interfaces;
using BankApp.Infrastructure.Repositories.Interfaces;
public class UserRepository : IUserRepository
{
    private readonly IUserDataAccess _userDao;
    private readonly ISessionDataAccess _sessionDao;
    private readonly IOAuthLinkDataAccess _oAuthLinkDao;
    private readonly INotificationPreferenceDataAccess _notificationPreferenceDao;

    public UserRepository(IUserDataAccess userDao, ISessionDataAccess sessionDao, IOAuthLinkDataAccess oAuthLinkDao,
        INotificationPreferenceDataAccess notificationPreferenceDao)
    {
        _userDao = userDao;
        _sessionDao = sessionDao;
        _notificationPreferenceDao = notificationPreferenceDao;
        _oAuthLinkDao = oAuthLinkDao;
    }

    public User? FindById(int id)
    {
        return _userDao.FindById(id);
    }

    public bool UpdateUser(User user)
    {
        return _userDao.Update(user);
    }

    public bool UpdatePassword(int userId, string newPasswordHash)
    {
        return _userDao.UpdatePassword(userId, newPasswordHash);
    }

    public List<Session> GetActiveSessions(int userId)
    {
        return _sessionDao.FindByUserId(userId);
    }

    public void RevokeSession(int sessionId)
    {
        _sessionDao.Revoke(sessionId);
    }

    public List<OAuthLink> GetLinkedProviders(int userId)
    {
        return _oAuthLinkDao.FindByUserId(userId);
    }

    public bool SaveOAuthLink(int userId, string provider, string providerUserId, string? email)
    {
        return _oAuthLinkDao.Create(userId, provider, providerUserId, email);
    }

    public void DeleteOAuthLink(int linkId)
    {
        _oAuthLinkDao.Delete(linkId);
    }

    public List<NotificationPreference> GetNotificationPreferences(int userId)
    {
        return _notificationPreferenceDao.FindByUserId(userId);
    }

    public bool UpdateNotificationPreferences(int userId, List<NotificationPreference> prefs)
    {
        return _notificationPreferenceDao.Update(userId, prefs);
    }
}


