using BankApp.Contracts.Entities;
using BankApp.Server.DataAccess.Interfaces;
using Dapper;
using ErrorOr;

namespace BankApp.Server.DataAccess.Implementations;

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

    private readonly AppDbContext db;

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionDataAccess"/> class.
    /// </summary>
    /// <param name="db">The database context used for executing queries.</param>
    public SessionDataAccess(AppDbContext db)
    {
        this.db = db;
    }

    /// <inheritdoc />
    public ErrorOr<Session> Create(int userId, string token, string? deviceInfo, string? browser, string? ipAddress)
    {
        const string sql = """
            INSERT INTO [Session] (UserId, Token, DeviceInfo, Browser, IpAddress, LastActiveAt, ExpiresAt)
            OUTPUT INSERTED.Id, INSERTED.UserId, INSERTED.Token, INSERTED.DeviceInfo,
                   INSERTED.Browser, INSERTED.IpAddress, INSERTED.LastActiveAt,
                   INSERTED.ExpiresAt, INSERTED.IsRevoked, INSERTED.CreatedAt
            VALUES (@UserId, @Token, @DeviceInfo, @Browser, @IpAddress,
                    GETUTCDATE(), DATEADD(DAY, @ExpirationDays, GETUTCDATE()))
            """;

        return this.db.Query(conn => conn.QueryFirstOrDefault<Session>(sql, new
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
        return this.db.Query(conn => conn.QueryFirstOrDefault<Session>(query, new { Token = token }))
            .Then(session => session ?? (ErrorOr<Session>)Error.NotFound(description: "Session not found."));
    }

    /// <inheritdoc />
    public ErrorOr<List<Session>> FindByUserId(int userId)
    {
        const string query = $"{SelectAllColumns} WHERE UserId = @UserId AND IsRevoked = 0 AND ExpiresAt > GETUTCDATE()";
        return this.db.Query(conn => conn.Query<Session>(query, new { UserId = userId }).AsList());
    }

    /// <inheritdoc />
    public ErrorOr<Success> Revoke(int sessionId)
    {
        const string sql = "UPDATE [Session] SET IsRevoked = 1 WHERE Id = @Id";
        return this.db.Query(conn => conn.Execute(sql, new { Id = sessionId }))
            .Then(_ => (ErrorOr<Success>)Result.Success);
    }

    /// <inheritdoc />
    public ErrorOr<Success> RevokeAll(int userId)
    {
        const string sql = "UPDATE [Session] SET IsRevoked = 1 WHERE UserId = @UserId AND IsRevoked = 0";
        return this.db.Query(conn => conn.Execute(sql, new { UserId = userId }))
            .Then(_ => (ErrorOr<Success>)Result.Success);
    }
}
