using BankApp.Core.Entities;
namespace BankApp.Infrastructure.DataAccess.Interfaces
{
    /// <summary>
    /// Defines data access operations for user sessions.
    /// </summary>
    public interface ISessionDataAccess
    {
        /// <summary>
        /// Creates a new session for the specified user.
        /// </summary>
        /// <param name="userId">The identifier of the user.</param>
        /// <param name="token">The unique session token.</param>
        /// <param name="deviceInfo">Optional device information.</param>
        /// <param name="browser">Optional browser name.</param>
        /// <param name="ip">Optional IP address.</param>
        /// <returns>The newly created <see cref="Session"/>.</returns>
        Session Create(int userId, string token, string? deviceInfo, string? browser, string? ip);

        /// <summary>
        /// Finds an active session by its token.
        /// </summary>
        /// <param name="token">The session token to search for.</param>
        /// <returns>The matching <see cref="Session"/>, or <see langword="null"/> if not found or expired.</returns>
        Session? FindByToken(string token);

        /// <summary>
        /// Finds all active sessions for the specified user.
        /// </summary>
        /// <param name="userId">The identifier of the user.</param>
        /// <returns>A list of active sessions for the user.</returns>
        List<Session> FindByUserId(int userId);

        /// <summary>
        /// Revokes a single session by its identifier.
        /// </summary>
        /// <param name="sessionId">The session identifier.</param>
        void Revoke(int sessionId);

        /// <summary>
        /// Revokes all active sessions for the specified user.
        /// </summary>
        /// <param name="userId">The identifier of the user.</param>
        void RevokeAll(int userId);
    }
}


