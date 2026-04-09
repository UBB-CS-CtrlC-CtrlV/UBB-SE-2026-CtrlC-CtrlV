using BankApp.Contracts.Entities;
using BankApp.Contracts.Enums;
using BankApp.Contracts.Extensions;
using BankApp.Server.DataAccess.Interfaces;
using BankApp.Server.Repositories.Interfaces;
using ErrorOr;

namespace BankApp.Server.Repositories.Implementations;

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
    public ErrorOr<User> FindUserByEmail(string email) => userDataAccess.FindByEmail(email);

    /// <inheritdoc />
    public ErrorOr<User> FindUserById(int id) => userDataAccess.FindById(id);

    /// <inheritdoc />
    public ErrorOr<Success> CreateUser(User user)
    {
        var createResult = userDataAccess.Create(user);
        if (createResult.IsError)
        {
            return createResult.FirstError;
        }

        var createdUser = userDataAccess.FindByEmail(user.Email);
        if (createdUser.IsError)
        {
            return createdUser.FirstError;
        }

        foreach (NotificationType type in Enum.GetValues(typeof(NotificationType)))
        {
            var prefResult = notificationPreferenceDataAccess.Create(createdUser.Value.Id, type.ToDisplayName());
            if (prefResult.IsError)
            {
                return prefResult.FirstError;
            }
        }

        return Result.Success;
    }

    /// <inheritdoc />
    public ErrorOr<OAuthLink> FindOAuthLink(string provider, string providerUserId) =>
        oAuthLinkDataAccess.FindByProvider(provider, providerUserId);

    /// <inheritdoc />
    public ErrorOr<Success> CreateOAuthLink(OAuthLink link) =>
        oAuthLinkDataAccess.Create(link.UserId, link.Provider, link.ProviderUserId, link.ProviderEmail);

    /// <inheritdoc />
    public ErrorOr<Session> CreateSession(int userId, string token, string? deviceInfo, string? browser, string? ipAddress) =>
        sessionDataAccess.Create(userId, token, deviceInfo, browser, ipAddress);

    /// <inheritdoc />
    public ErrorOr<Session> FindSessionByToken(string token) => sessionDataAccess.FindByToken(token);

    /// <inheritdoc />
    public ErrorOr<List<Session>> FindSessionsByUserId(int userId) => sessionDataAccess.FindByUserId(userId);

    /// <inheritdoc />
    public ErrorOr<Success> UpdateSessionToken(int sessionId) => sessionDataAccess.Revoke(sessionId);

    /// <inheritdoc />
    public ErrorOr<Success> InvalidateAllSessions(int userId) => sessionDataAccess.RevokeAll(userId);

    /// <inheritdoc />
    public ErrorOr<Success> SavePasswordResetToken(PasswordResetToken token) =>
        passwordResetTokenDataAccess.Create(token.UserId, token.TokenHash, token.ExpiresAt)
            .Then(_ => Result.Success);

    /// <inheritdoc />
    public ErrorOr<PasswordResetToken> FindPasswordResetToken(string tokenHash) =>
        passwordResetTokenDataAccess.FindByToken(tokenHash);

    /// <inheritdoc />
    public ErrorOr<Success> MarkPasswordResetTokenAsUsed(int tokenId) =>
        passwordResetTokenDataAccess.MarkAsUsed(tokenId);

    /// <inheritdoc />
    public ErrorOr<Success> DeleteExpiredPasswordResetTokens() =>
        passwordResetTokenDataAccess.DeleteExpired();

    /// <inheritdoc />
    public ErrorOr<Success> IncrementFailedAttempts(int userId) => userDataAccess.IncrementFailedAttempts(userId);

    /// <inheritdoc />
    public ErrorOr<Success> ResetFailedAttempts(int userId) => userDataAccess.ResetFailedAttempts(userId);

    /// <inheritdoc />
    public ErrorOr<Success> LockAccount(int userId, DateTime lockoutEnd) => userDataAccess.LockAccount(userId, lockoutEnd);

    /// <inheritdoc />
    public ErrorOr<Success> UpdatePassword(int userId, string newPasswordHash) =>
        userDataAccess.UpdatePassword(userId, newPasswordHash);
}
