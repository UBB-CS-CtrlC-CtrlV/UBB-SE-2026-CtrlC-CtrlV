using BankApp.Domain.Entities;
using BankApp.Infrastructure.DataAccess.Interfaces;
using Dapper;
using ErrorOr;

namespace BankApp.Infrastructure.DataAccess.Implementations;

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

    private readonly AppDatabaseContext databaseContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationDataAccess"/> class.
    /// </summary>
    /// <param name="databaseContext">The database context used for executing queries.</param>
    public NotificationDataAccess(AppDatabaseContext databaseContext)
    {
        this.databaseContext = databaseContext;
    }

    /// <inheritdoc />
    public ErrorOr<int> CountUnreadByUserId(int userId)
    {
        const string query = "SELECT COUNT(*) FROM Notification WHERE UserId = @UserId AND IsRead = 0";
        return this.databaseContext.Query(connection => connection.QueryFirst<int>(query, new { UserId = userId }));
    }

    /// <inheritdoc />
    public ErrorOr<List<Notification>> FindByUserId(int userId)
    {
        const string query = $"{SelectAllColumns} WHERE UserId = @UserId";
        return this.databaseContext.Query(connection => connection.Query<Notification>(query, new { UserId = userId }).AsList());
    }
}
