using BankApp.Contracts.Entities;
using BankApp.Server.DataAccess.Interfaces;
using BankApp.Server.Repositories.Interfaces;
using ErrorOr;

namespace BankApp.Server.Repositories.Implementations;

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
    public ErrorOr<User> FindById(int id) => userDataAccess.FindById(id);

    /// <inheritdoc />
    public ErrorOr<Success> UpdateUser(User user) => userDataAccess.Update(user);

    /// <inheritdoc />
    public ErrorOr<Success> UpdatePassword(int userId, string newPasswordHash) =>
        userDataAccess.UpdatePassword(userId, newPasswordHash);

    /// <inheritdoc />
    public ErrorOr<List<Session>> GetActiveSessions(int userId) => sessionDataAccess.FindByUserId(userId);

    /// <inheritdoc />
    public ErrorOr<Success> RevokeSession(int userId, int sessionId) => sessionDataAccess.RevokeForUser(userId, sessionId);

    /// <inheritdoc />
    public ErrorOr<List<OAuthLink>> GetLinkedProviders(int userId) => oAuthLinkDataAccess.FindByUserId(userId);

    /// <inheritdoc />
    public ErrorOr<Success> SaveOAuthLink(int userId, string provider, string providerUserId, string? email) =>
        oAuthLinkDataAccess.Create(userId, provider, providerUserId, email);

    /// <inheritdoc />
    public ErrorOr<Success> DeleteOAuthLink(int linkId) => oAuthLinkDataAccess.Delete(linkId);

    /// <inheritdoc />
    public ErrorOr<List<NotificationPreference>> GetNotificationPreferences(int userId) =>
        notificationPreferenceDataAccess.FindByUserId(userId);

    /// <inheritdoc />
    public ErrorOr<Success> UpdateNotificationPreferences(int userId, List<NotificationPreference> preferences) =>
        notificationPreferenceDataAccess.Update(userId, preferences);
}
