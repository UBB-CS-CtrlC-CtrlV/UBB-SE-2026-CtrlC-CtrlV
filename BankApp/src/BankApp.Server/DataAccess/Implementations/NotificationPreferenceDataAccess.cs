using BankApp.Contracts.Entities;
using BankApp.Server.DataAccess.Interfaces;
using BankApp.Contracts.Extensions;
using Dapper;
using ErrorOr;

namespace BankApp.Server.DataAccess.Implementations;

/// <summary>
/// Provides SQL Server data access for notification preference records.
/// </summary>
internal class NotificationPreferenceDataAccess : INotificationPreferenceDataAccess
{
    private readonly AppDbContext appDbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationPreferenceDataAccess"/> class.
    /// </summary>
    /// <param name="appDbContext">The database context used for executing queries.</param>
    public NotificationPreferenceDataAccess(AppDbContext appDbContext)
    {
        this.appDbContext = appDbContext;
    }

    /// <inheritdoc />
    public ErrorOr<Success> Create(int userId, string category)
    {
        try
        {
            const string query = """
                                 INSERT INTO NotificationPreference (UserId, Category, PushEnabled, EmailEnabled, SmsEnabled) VALUES
                                   (@UserId, @Category, 0, 0, 0);
                                                                    
                                 """;

            var rows = this.appDbContext.GetConnection()
                .Execute(query, new { UserId = userId, Category = category });

            return rows > 0 ? Result.Success : Error.Failure(description: "Didn't manage to insert any rows");
        }
        catch (Exception ex)
        {
            return Error.Failure(description: $"Insertion failed: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public List<NotificationPreference> FindByUserId(int userId)
    {
        const string query = """
                             SELECT 
                                 Id, UserId, Category, PushEnabled,
                                 EmailEnabled, SmsEnabled, MinAmountThreshold
                             FROM NotificationPreference WHERE UserId = @UserId
                             """;
        return this.appDbContext.GetConnection().Query<NotificationPreference>(query, new { UserId = userId }).AsList();
    }

    /// <inheritdoc />
    public ErrorOr<Success> Update(int userId, List<NotificationPreference> preferences)
    {
        const string query = """
                             UPDATE NotificationPreference
                             SET PushEnabled = @PushEnabled, 
                                 EmailEnabled = @EmailEnabled, 
                                 SmsEnabled = @SmsEnabled,
                                 MinAmountThreshold = @MinAmountThreshold
                             WHERE UserId = @UserId
                             AND Category = @Category
                             """;
        try
        {
            this.appDbContext.GetConnection().Execute(query, preferences.Select(p => new
            {
                p.UserId,
                Category = p.Category.ToDisplayName(),
                p.PushEnabled,
                p.EmailEnabled,
                p.SmsEnabled,
                p.MinAmountThreshold,
            }));
            return Result.Success;
        }
        catch (Exception ex)
        {
            return Error.Failure(description: $"Update failed: {ex.Message}");
        }
    }
}