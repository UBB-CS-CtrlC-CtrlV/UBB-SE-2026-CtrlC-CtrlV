using BankApp.Domain.Entities;
using ErrorOr;

namespace BankApp.Infrastructure.DataAccess.Interfaces;

/// <summary>
/// Defines data access operations for user sessions.
/// </summary>
public interface ISessionDataAccess
{
    /// <summary>Creates a new session for the specified user.</summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="token">The unique session token.</param>
    /// <param name="deviceInfo">Optional device information.</param>
    /// <param name="browser">Optional browser name.</param>
    /// <param name="ipAddress">Optional IP address.</param>
    /// <returns>The newly created <see cref="Session"/>, or an error if the operation failed.</returns>
    ErrorOr<Session> Create(int userId, string token, string? deviceInfo, string? browser, string? ipAddress);

    /// <summary>Finds an active session by its token.</summary>
    /// <param name="token">The session token to search for.</param>
    /// <returns>The matching <see cref="Session"/>, or <see cref="Error.NotFound"/> if not found or expired.</returns>
    ErrorOr<Session> FindByToken(string token);

    /// <summary>Finds all active sessions for the specified user.</summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <returns>A list of active sessions for the user, or an error if the operation failed.</returns>
    ErrorOr<List<Session>> FindByUserId(int userId);

    /// <summary>Revokes a single session by its identifier.</summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <returns>Success, or an error if the operation failed.</returns>
    ErrorOr<Success> Revoke(int sessionId);

    /// <summary>Revokes a single active session belonging to the specified user.</summary>
    /// <param name="userId">The identifier of the user who owns the session.</param>
    /// <param name="sessionId">The session identifier.</param>
    /// <returns>Success, or a not-found error if no active matching session exists.</returns>
    ErrorOr<Success> RevokeForUser(int userId, int sessionId);

    /// <summary>Revokes all active sessions for the specified user.</summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <returns>Success, or an error if the operation failed.</returns>
    ErrorOr<Success> RevokeAll(int userId);
}
