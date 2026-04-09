using BankApp.Contracts.Entities;
using BankApp.Server.DataAccess.Interfaces;
using Dapper;
using ErrorOr;

namespace BankApp.Server.DataAccess.Implementations;

/// <summary>
/// Provides SQL Server data access for notification records.
/// </summary>
public class NotificationDataAccess : INotificationDataAccess
{
    private const string SelectAllColumns = """
        SELECT Id, UserId, Title, [Message], [Type], Channel,
               IsRead, RelatedEntityType, RelatedEntityId, CreatedAt
        FROM Notification
        """;

    private readonly AppDbContext db;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationDataAccess"/> class.
    /// </summary>
    /// <param name="db">The database context used for executing queries.</param>
    public NotificationDataAccess(AppDbContext db)
    {
        this.db = db;
    }

    /// <inheritdoc />
    public ErrorOr<int> CountUnreadByUserId(int userId)
    {
        const string query = "SELECT COUNT(*) FROM Notification WHERE UserId = @UserId AND IsRead = 0";
        return this.db.Query(conn => conn.QueryFirst<int>(query, new { UserId = userId }));
    }

    /// <inheritdoc />
    public ErrorOr<List<Notification>> FindByUserId(int userId)
    {
        const string query = $"{SelectAllColumns} WHERE UserId = @UserId";
        return this.db.Query(conn => conn.Query<Notification>(query, new { UserId = userId }).AsList());
    }
}
