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
        private readonly IUserDataAccess userDataAccess;
        private readonly ISessionDataAccess sessionDataAccess;
        private readonly IOAuthLinkDataAccess oAuthLinkDataAccess;
        private readonly IPasswordResetTokenDataAccess passwordResetTokenDataAccess;
        private readonly INotificationPreferenceDataAccess notificationPreferenceDataAccess;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthRepository"/> class.
        /// </summary>
        /// <param name="userDataAccess">The user data access component.</param>
        /// <param name="sessionDataAccess">The session data access component.</param>
        /// <param name="oAuthLinkDataAccess">The OAuth link data access component.</param>
        /// <param name="passwordResetTokenDataAccess">The password reset token data access component.</param>
        /// <param name="notificationPreferenceDataAccess">The notification preference data access component.</param>
        public AuthRepository(IUserDataAccess userDataAccess, ISessionDataAccess sessionDataAccess, IOAuthLinkDataAccess oAuthLinkDataAccess,
            IPasswordResetTokenDataAccess passwordResetTokenDataAccess, INotificationPreferenceDataAccess notificationPreferenceDataAccess)
        {
            this.userDataAccess = userDataAccess;
            this.sessionDataAccess = sessionDataAccess;
            this.oAuthLinkDataAccess = oAuthLinkDataAccess;
            this.passwordResetTokenDataAccess = passwordResetTokenDataAccess;
            this.notificationPreferenceDataAccess = notificationPreferenceDataAccess;
        }

        /// <inheritdoc />
        public User? FindUserByEmail(string email)
        {
            return userDataAccess.FindByEmail(email);
        }

        /// <inheritdoc />
        public bool CreateUser(User user)
        {
            bool success = userDataAccess.Create(user);
            if (!success)
            {
                return false;
            }

            User? createdUser = userDataAccess.FindByEmail(user.Email);
            if (createdUser == null)
            {
                return false;
            }

            foreach (NotificationType type in Enum.GetValues(typeof(NotificationType)))
            {
                success = notificationPreferenceDataAccess.Create(createdUser.Id, NotificationTypeExtensions.ToDisplayName(type));
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
            return oAuthLinkDataAccess.FindByProvider(provider, providerUserId);
        }

        /// <inheritdoc />
        public bool CreateOAuthLink(OAuthLink link)
        {
            return oAuthLinkDataAccess.Create(link.UserId, link.Provider, link.ProviderUserId, link.ProviderEmail);
        }

        /// <inheritdoc />
        public Session CreateSession(int userId, string token, string? deviceInfo, string? browser, string? ipAddress)
        {
            return sessionDataAccess.Create(userId, token, deviceInfo, browser, ipAddress);
        }

        /// <inheritdoc />
        public Session? FindSessionByToken(string token)
        {
            return sessionDataAccess.FindByToken(token);
        }

        /// <inheritdoc />
        public void InvalidateAllSessions(int userId)
        {
            sessionDataAccess.RevokeAll(userId);
        }

        /// <inheritdoc />
        public List<Session> FindSessionsByUserId(int userId)
        {
            return sessionDataAccess.FindByUserId(userId);
        }

        /// <inheritdoc />
        public bool UpdateSessionToken(int sessionId)
        {
            // Revoke the old session
            // the service layer will create a new one
            sessionDataAccess.Revoke(sessionId);
            return true;
        }

        /// <inheritdoc />
        public void SavePasswordResetToken(PasswordResetToken token)
        {
            passwordResetTokenDataAccess.Create(token.UserId, token.TokenHash, token.ExpiresAt);
        }

        /// <inheritdoc />
        public PasswordResetToken? FindPasswordResetToken(string tokenHash)
        {
            return passwordResetTokenDataAccess.FindByToken(tokenHash);
        }

        /// <inheritdoc />
        public void MarkPasswordResetTokenAsUsed(int tokenId)
        {
            passwordResetTokenDataAccess.MarkAsUsed(tokenId);
        }

        /// <inheritdoc />
        public void IncrementFailedAttempts(int userId)
        {
            userDataAccess.IncrementFailedAttempts(userId);
        }

        /// <inheritdoc />
        public void ResetFailedAttempts(int userId)
        {
            userDataAccess.ResetFailedAttempts(userId);
        }

        /// <inheritdoc />
        public void LockAccount(int userId, DateTime lockoutEnd)
        {
            userDataAccess.LockAccount(userId, lockoutEnd);
        }

        /// <inheritdoc />
        public User? FindUserById(int id)
        {
            return userDataAccess.FindById(id);
        }

        /// <inheritdoc />
        public bool UpdatePassword(int userId, string newPasswordHash)
        {
            return userDataAccess.UpdatePassword(userId, newPasswordHash);
        }
    }
}


