using BankApp.Infrastructure.Repositories.Interfaces;
using BankApp.Infrastructure.DataAccess.Interfaces;
using BankApp.Core.Entities;
using BankApp.Core.Enums;
using BankApp.Core.Extensions;

namespace BankApp.Infrastructure.Repositories.Implementations
{
    /// <summary>
    /// Provides repository operations for authentication, session management, and account security.
    /// </summary>
    public class AuthRepository : IAuthRepository
    {
        private readonly IUserDataAccess userDao;
        private readonly ISessionDataAccess sessionDao;
        private readonly IOAuthLinkDataAccess oAuthLinkDao;
        private readonly IPasswordResetTokenDataAccess passwordResetTokenDao;
        private readonly INotificationPreferenceDataAccess notificationPreferenceDao;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthRepository"/> class.
        /// </summary>
        /// <param name="userDao">The user data access component.</param>
        /// <param name="sessionDao">The session data access component.</param>
        /// <param name="oAuthLinkDao">The OAuth link data access component.</param>
        /// <param name="passwordResetTokenDao">The password reset token data access component.</param>
        /// <param name="notificationPreferenceDao">The notification preference data access component.</param>
        public AuthRepository(IUserDataAccess userDao, ISessionDataAccess sessionDao, IOAuthLinkDataAccess oAuthLinkDao,
            IPasswordResetTokenDataAccess passwordResetTokenDao, INotificationPreferenceDataAccess notificationPreferenceDao)
        {
            this.userDao = userDao;
            this.sessionDao = sessionDao;
            this.oAuthLinkDao = oAuthLinkDao;
            this.passwordResetTokenDao = passwordResetTokenDao;
            this.notificationPreferenceDao = notificationPreferenceDao;
        }

        /// <inheritdoc />
        public User? FindUserByEmail(string email)
        {
            return userDao.FindByEmail(email);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public OAuthLink? FindOAuthLink(string provider, string providerUserId)
        {
            return oAuthLinkDao.FindByProvider(provider, providerUserId);
        }

        /// <inheritdoc />
        public bool CreateOAuthLink(OAuthLink link)
        {
            return oAuthLinkDao.Create(link.UserId, link.Provider, link.ProviderUserId, link.ProviderEmail);
        }

        /// <inheritdoc />
        public Session CreateSession(int userId, string token, string? deviceInfo, string? browser, string? ip)
        {
            return sessionDao.Create(userId, token, deviceInfo, browser, ip);
        }

        /// <inheritdoc />
        public Session? FindSessionByToken(string token)
        {
            return sessionDao.FindByToken(token);
        }

        /// <inheritdoc />
        public void InvalidateAllSessions(int userId)
        {
            sessionDao.RevokeAll(userId);
        }

        /// <inheritdoc />
        public List<Session> FindSessionsByUserId(int userId)
        {
            return sessionDao.FindByUserId(userId);
        }

        /// <inheritdoc />
        public bool UpdateSessionToken(int sessionId)
        {
            // Revoke the old session
            // the service layer will create a new one
            sessionDao.Revoke(sessionId);
            return true;
        }

        /// <inheritdoc />
        public void SavePasswordResetToken(PasswordResetToken token)
        {
            passwordResetTokenDao.Create(token.UserId, token.TokenHash, token.ExpiresAt);
        }

        /// <inheritdoc />
        public PasswordResetToken? FindPasswordResetToken(string tokenHash)
        {
            return passwordResetTokenDao.FindByToken(tokenHash);
        }

        /// <inheritdoc />
        public void MarkPasswordResetTokenAsUsed(int tokenId)
        {
            passwordResetTokenDao.MarkAsUsed(tokenId);
        }

        /// <inheritdoc />
        public void IncrementFailedAttempts(int userId)
        {
            userDao.IncrementFailedAttempts(userId);
        }

        /// <inheritdoc />
        public void ResetFailedAttempts(int userId)
        {
            userDao.ResetFailedAttempts(userId);
        }

        /// <inheritdoc />
        public void LockAccount(int userId, DateTime lockoutEnd)
        {
            userDao.LockAccount(userId, lockoutEnd);
        }

        /// <inheritdoc />
        public User? FindUserById(int id)
        {
            return userDao.FindById(id);
        }

        /// <inheritdoc />
        public bool UpdatePassword(int userId, string newPasswordHash)
        {
            return userDao.UpdatePassword(userId, newPasswordHash);
        }
    }
}


