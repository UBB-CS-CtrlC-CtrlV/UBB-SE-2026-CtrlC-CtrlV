using BankApp.Domain.Entities;
using BankApp.Infrastructure.DataAccess.Interfaces;
using Dapper;
using ErrorOr;

namespace BankApp.Infrastructure.DataAccess.Implementations;

/// <summary>
/// Provides SQL Server data access for user session records.
/// </summary>
public class SessionDataAccess : ISessionDataAccess
{
    private const int SessionExpirationDays = 7;

    private const string SelectAllColumns = """
        SELECT Id, UserId, Token, DeviceInfo, Browser, IpAddress,
               LastActiveAt, ExpiresAt, IsRevoked, CreatedAt
        FROM [Session]
        """;

    private readonly AppDatabaseContext databaseContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionDataAccess"/> class.
    /// </summary>
    /// <param name="databaseContext">The database context used for executing queries.</param>
    public SessionDataAccess(AppDatabaseContext databaseContext)
    {
        this.databaseContext = databaseContext;
    }

    /// <inheritdoc />
    public ErrorOr<Session> Create(int userId, string token, string? deviceInfo, string? browser, string? ipAddress)
    {
        const string databaseCommandText = """
            INSERT INTO [Session] (UserId, Token, DeviceInfo, Browser, IpAddress, LastActiveAt, ExpiresAt)
            OUTPUT INSERTED.Id, INSERTED.UserId, INSERTED.Token, INSERTED.DeviceInfo,
                   INSERTED.Browser, INSERTED.IpAddress, INSERTED.LastActiveAt,
                   INSERTED.ExpiresAt, INSERTED.IsRevoked, INSERTED.CreatedAt
            VALUES (@UserId, @Token, @DeviceInfo, @Browser, @IpAddress,
                    GETUTCDATE(), DATEADD(DAY, @ExpirationDays, GETUTCDATE()))
            """;

        return this.databaseContext.Query(connection => connection.QueryFirstOrDefault<Session>(databaseCommandText, new
        {
            UserId = userId,
            Token = token,
            DeviceInfo = deviceInfo,
            Browser = browser,
            IpAddress = ipAddress,
            ExpirationDays = SessionExpirationDays,
        })).Then(session => session ?? (ErrorOr<Session>)Error.Failure(description: "Failed to create session."));
    }

    /// <inheritdoc />
    public ErrorOr<Session> FindByToken(string token)
    {
        const string query = $"{SelectAllColumns} WHERE Token = @Token AND IsRevoked = 0 AND ExpiresAt > GETUTCDATE()";
        return this.databaseContext.Query(connection => connection.QueryFirstOrDefault<Session>(query, new { Token = token }))
            .Then(session => session ?? (ErrorOr<Session>)Error.NotFound(description: "Session not found."));
    }

    /// <inheritdoc />
    public ErrorOr<List<Session>> FindByUserId(int userId)
    {
        const string query = $"{SelectAllColumns} WHERE UserId = @UserId AND IsRevoked = 0 AND ExpiresAt > GETUTCDATE()";
        return this.databaseContext.Query(connection => connection.Query<Session>(query, new { UserId = userId }).AsList());
    }

    /// <inheritdoc />
    public ErrorOr<Success> Revoke(int sessionId)
    {
        const string databaseCommandText = "UPDATE [Session] SET IsRevoked = 1 WHERE Id = @Id";
        return this.databaseContext.Query(connection => connection.Execute(databaseCommandText, new { Id = sessionId }))
            .Then(_ => (ErrorOr<Success>)Result.Success);
    }

    /// <inheritdoc />
    public ErrorOr<Success> RevokeForUser(int userId, int sessionId)
    {
        const string databaseCommandText = """
            UPDATE [Session]
            SET IsRevoked = 1
            WHERE Id = @SessionId
              AND UserId = @UserId
              AND IsRevoked = 0
              AND ExpiresAt > GETUTCDATE()
            """;

        return this.databaseContext.Query(connection => connection.Execute(databaseCommandText, new { UserId = userId, SessionId = sessionId }))
            .Then(rowsAffected => rowsAffected > default(int)
                ? Result.Success
                : (ErrorOr<Success>)Error.NotFound(description: "Session not found."));
    }

    /// <inheritdoc />
    public ErrorOr<Success> RevokeAll(int userId)
    {
        const string databaseCommandText = "UPDATE [Session] SET IsRevoked = 1 WHERE UserId = @UserId AND IsRevoked = 0";
        return this.databaseContext.Query(connection => connection.Execute(databaseCommandText, new { UserId = userId }))
            .Then(_ => (ErrorOr<Success>)Result.Success);
    }
}
