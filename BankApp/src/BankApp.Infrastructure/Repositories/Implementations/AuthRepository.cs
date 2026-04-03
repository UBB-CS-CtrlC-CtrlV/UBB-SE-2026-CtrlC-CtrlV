using BankApp.Infrastructure.Repositories.Interfaces;
using BankApp.Infrastructure.DataAccess.Interfaces;
using BankApp.Core.Entities;
using BankApp.Core.Enums;
using BankApp.Core.Extensions;

namespace BankApp.Infrastructure.Repositories.Implementations
{
    public class AuthRepository : IAuthRepository
    {
        private readonly IUserDataAccess userDao;
        private readonly ISessionDataAccess sessionDao;
        private readonly IOAuthLinkDataAccess oAuthLinkDao;
        private readonly IPasswordResetTokenDataAccess passwordResetTokenDao;
        private readonly INotificationPreferenceDataAccess notificationPreferenceDao;

        public AuthRepository(IUserDataAccess userDao, ISessionDataAccess sessionDao, IOAuthLinkDataAccess oAuthLinkDao,
            IPasswordResetTokenDataAccess passwordResetTokenDao, INotificationPreferenceDataAccess notificationPreferenceDao)
        {
            this.userDao = userDao;
            this.sessionDao = sessionDao;
            this.oAuthLinkDao = oAuthLinkDao;
            this.passwordResetTokenDao = passwordResetTokenDao;
            this.notificationPreferenceDao = notificationPreferenceDao;
        }

        // USER
        public User? FindUserByEmail(string email)
        {
            return userDao.FindByEmail(email);
        }

        public bool CreateUser(User user)
        {
            bool success = userDao.Create(user);
            if (!success)
            {
                return false;
            }

            User? createdUser = userDao.FindByEmail(user.Email);
            if (createdUser == null)
            {
                return false;
            }

            foreach (NotificationType type in Enum.GetValues(typeof(NotificationType)))
            {
                success = notificationPreferenceDao.Create(createdUser.Id, NotificationTypeExtensions.ToDisplayName(type));
                if (!success)
                {
                    return false;
                }
            }

            return true;
        }

        // OAUTH
        public OAuthLink? FindOAuthLink(string provider, string providerUserId)
        {
            return oAuthLinkDao.FindByProvider(provider, providerUserId);
        }

        public bool CreateOAuthLink(OAuthLink link)
        {
            return oAuthLinkDao.Create(link.UserId, link.Provider, link.ProviderUserId, link.ProviderEmail);
        }

        // SESSIONS
        public Session CreateSession(int userId, string token, string? deviceInfo, string? browser, string? ip)
        {
            return sessionDao.Create(userId, token, deviceInfo, browser, ip);
        }

        public Session? FindSessionByToken(string token)
        {
            return sessionDao.FindByToken(token);
        }

        public void InvalidateAllSessions(int userId)
        {
            sessionDao.RevokeAll(userId);
        }

        public List<Session> FindSessionsByUserId(int userId)
        {
            return sessionDao.FindByUserId(userId);
        }

        public bool UpdateSessionToken(int sessionId)
        {
            // Revoke the old session
            // the service layer will create a new one
            sessionDao.Revoke(sessionId);
            return true;
        }

        public void SavePasswordResetToken(PasswordResetToken token)
        {
            passwordResetTokenDao.Create(token.UserId, token.TokenHash, token.ExpiresAt);
        }

        public PasswordResetToken? FindPasswordResetToken(string tokenHash)
        {
            return passwordResetTokenDao.FindByToken(tokenHash);
        }

        public void MarkPasswordResetTokenAsUsed(int tokenId)
        {
            passwordResetTokenDao.MarkAsUsed(tokenId);
        }

        // ACCOUNT SECURITY
        public void IncrementFailedAttempts(int userId)
        {
            userDao.IncrementFailedAttempts(userId);
        }

        public void ResetFailedAttempts(int userId)
        {
            userDao.ResetFailedAttempts(userId);
        }

        public void LockAccount(int userId, DateTime lockoutEnd)
        {
            userDao.LockAccount(userId, lockoutEnd);
        }

        public User? FindUserById(int id)
        {
            return userDao.FindById(id);
        }

        public bool UpdatePassword(int userId, string newPasswordHash)
        {
            return userDao.UpdatePassword(userId, newPasswordHash);
        }
    }
}


