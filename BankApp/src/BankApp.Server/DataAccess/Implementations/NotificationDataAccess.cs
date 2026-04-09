using BankApp.Contracts.Entities;
using BankApp.Server.DataAccess.Interfaces;
using Dapper;

namespace BankApp.Server.DataAccess.Implementations;

/// <summary>
/// Provides SQL Server data access for notification records.
/// </summary>
public class NotificationDataAccess : INotificationDataAccess
{
    private readonly AppDbContext dbContext;

    private const string SelectAllColumns = """
                                                SELECT
                                                    Id, UserId, Title, [Message], [Type], Channel, 
                                                    IsRead, RelatedEntityType, RelatedEntityId, CreatedAt
                                                FROM Notification
                                            """;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationDataAccess"/> class.
    /// </summary>
    /// <param name="dbContext">The database context used for executing queries.</param>
    public NotificationDataAccess(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public int CountUnreadByUserId(int userId)
    {
        const string query = "SELECT COUNT(*) FROM Notification WHERE UserId = @UserId AND IsRead = 0";

        return this.dbContext.GetConnection().QueryFirst<int>(query, new { UserId = userId });
    }

    /// <inheritdoc />
    public List<Notification> FindByUserId(int userId)
    {
        var query = $"{SelectAllColumns} WHERE UserId = @UserId";

        return this.dbContext.GetConnection().Query<Notification>(query, new { UserId = userId }).AsList();
    }
}