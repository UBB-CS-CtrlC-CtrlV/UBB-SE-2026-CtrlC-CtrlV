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

    private readonly AppDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionDataAccess"/> class.
    /// </summary>
    /// <param name="dbContext">The database context used for executing queries.</param>
    public SessionDataAccess(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
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

        var session = this.dbContext.GetConnection().QueryFirstOrDefault<Session>(sql, new
        {
            UserId = userId,
            Token = token,
            DeviceInfo = deviceInfo,
            Browser = browser,
            IpAddress = ipAddress,
            ExpirationDays = SessionExpirationDays,
        });

        return session is null
            ? Error.Failure(description: "Failed to create session.")
            : session;
    }

    /// <inheritdoc />
    public Session? FindByToken(string token)
    {
        const string query = $"{SelectAllColumns} WHERE Token = @Token AND IsRevoked = 0 AND ExpiresAt > GETUTCDATE()";
        return this.dbContext.GetConnection().QueryFirstOrDefault<Session>(query, new { Token = token });
    }

    /// <inheritdoc />
    public List<Session> FindByUserId(int userId)
    {
        const string query = $"{SelectAllColumns} WHERE UserId = @UserId AND IsRevoked = 0 AND ExpiresAt > GETUTCDATE()";
        return this.dbContext.GetConnection().Query<Session>(query, new { UserId = userId }).AsList();
    }

    /// <inheritdoc />
    public void Revoke(int sessionId)
    {
        const string sql = "UPDATE [Session] SET IsRevoked = 1 WHERE Id = @Id";
        this.dbContext.GetConnection().Execute(sql, new { Id = sessionId });
    }

    /// <inheritdoc />
    public void RevokeAll(int userId)
    {
        const string sql = "UPDATE [Session] SET IsRevoked = 1 WHERE UserId = @UserId AND IsRevoked = 0";
        this.dbContext.GetConnection().Execute(sql, new { UserId = userId });
    }
}
