using BankApp.Contracts.Entities;
using BankApp.Contracts.Extensions;
using BankApp.Server.DataAccess.Interfaces;
using Dapper;
using ErrorOr;

namespace BankApp.Server.DataAccess.Implementations;

/// <summary>
/// Provides SQL Server data access for notification preference records.
/// </summary>
internal class NotificationPreferenceDataAccess : INotificationPreferenceDataAccess
{
    private readonly AppDbContext db;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationPreferenceDataAccess"/> class.
    /// </summary>
    /// <param name="db">The database context used for executing queries.</param>
    public NotificationPreferenceDataAccess(AppDbContext db)
    {
        this.db = db;
    }

    /// <inheritdoc />
    public ErrorOr<Success> Create(int userId, string category)
    {
        const string sql = """
            INSERT INTO NotificationPreference (UserId, Category, PushEnabled, EmailEnabled, SmsEnabled)
            VALUES (@UserId, @Category, 0, 0, 0)
            """;

        return this.db.Query(conn => conn.Execute(sql, new { UserId = userId, Category = category }))
            .Then(rows => rows > 0 ? Result.Success : (ErrorOr<Success>)Error.Failure(description: "Failed to create notification preference."));
    }

    /// <inheritdoc />
    public ErrorOr<List<NotificationPreference>> FindByUserId(int userId)
    {
        const string query = """
            SELECT Id, UserId, Category, PushEnabled, EmailEnabled, SmsEnabled, MinAmountThreshold
            FROM NotificationPreference
            WHERE UserId = @UserId
            """;

        return this.db.Query(conn => conn.Query<NotificationPreference>(query, new { UserId = userId }).AsList());
    }

    /// <inheritdoc />
    public ErrorOr<Success> Update(int userId, List<NotificationPreference> preferences)
    {
        const string sql = """
            UPDATE NotificationPreference
            SET PushEnabled        = @PushEnabled,
                EmailEnabled       = @EmailEnabled,
                SmsEnabled         = @SmsEnabled,
                MinAmountThreshold = @MinAmountThreshold
            WHERE UserId   = @UserId
              AND Category = @Category
            """;

        return this.db.Query(conn => conn.Execute(sql, preferences.Select(p => new
        {
            p.UserId,
            Category = p.Category.ToDisplayName(),
            p.PushEnabled,
            p.EmailEnabled,
            p.SmsEnabled,
            p.MinAmountThreshold,
        })))
        .Then(_ => (ErrorOr<Success>)Result.Success);
    }
}
