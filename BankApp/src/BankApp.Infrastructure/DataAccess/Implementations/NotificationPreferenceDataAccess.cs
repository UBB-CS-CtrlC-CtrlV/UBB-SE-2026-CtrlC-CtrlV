using BankApp.Domain.Entities;
using BankApp.Domain.Extensions;
using BankApp.Infrastructure.DataAccess.Interfaces;
using Dapper;
using ErrorOr;

namespace BankApp.Infrastructure.DataAccess.Implementations;

/// <summary>
/// Provides SQL Server data access for notification preference records.
/// </summary>
internal class NotificationPreferenceDataAccess : INotificationPreferenceDataAccess
{
    private readonly AppDatabaseContext databaseContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationPreferenceDataAccess"/> class.
    /// </summary>
    /// <param name="databaseContext">The database context used for executing queries.</param>
    public NotificationPreferenceDataAccess(AppDatabaseContext databaseContext)
    {
        this.databaseContext = databaseContext;
    }

    /// <inheritdoc />
    public ErrorOr<Success> Create(int userId, string category)
    {
        const string databaseCommandText = """
            INSERT INTO NotificationPreference (UserId, Category, PushEnabled, EmailEnabled, SmsEnabled)
            VALUES (@UserId, @Category, 0, 0, 0)
            """;

        return this.databaseContext.Query(connection => connection.Execute(databaseCommandText, new { UserId = userId, Category = category }))
            .Then(rows => rows > default(int) ? Result.Success : (ErrorOr<Success>)Error.Failure(description: "Failed to create notification preference."));
    }

    /// <inheritdoc />
    public ErrorOr<List<NotificationPreference>> FindByUserId(int userId)
    {
        const string query = """
            SELECT Id, UserId, Category, PushEnabled, EmailEnabled, SmsEnabled, MinAmountThreshold
            FROM NotificationPreference
            WHERE UserId = @UserId
            """;

        return this.databaseContext.Query(connection => connection.Query<NotificationPreference>(query, new { UserId = userId }).AsList());
    }

    /// <inheritdoc />
    public ErrorOr<Success> Update(int userId, List<NotificationPreference> preferences)
    {
        const string databaseCommandText = """
            UPDATE NotificationPreference
            SET PushEnabled        = @PushEnabled,
                EmailEnabled       = @EmailEnabled,
                SmsEnabled         = @SmsEnabled,
                MinAmountThreshold = @MinAmountThreshold
            WHERE UserId   = @UserId
              AND Category = @Category
            """;

        return this.databaseContext.Query(connection => connection.Execute(databaseCommandText, preferences.Select(preference => new
        {
            preference.UserId,
            Category = preference.Category.ToDisplayName(),
            preference.PushEnabled,
            preference.EmailEnabled,
            preference.SmsEnabled,
            preference.MinAmountThreshold,
        })))
        .Then(_ => (ErrorOr<Success>)Result.Success);
    }
}
