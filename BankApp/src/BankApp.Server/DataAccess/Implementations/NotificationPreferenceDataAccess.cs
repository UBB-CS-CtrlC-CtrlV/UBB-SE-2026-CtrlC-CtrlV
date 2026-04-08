using BankApp.Contracts.Entities;
using BankApp.Server.DataAccess.Interfaces;
using BankApp.Contracts.Extensions;

namespace BankApp.Server.DataAccess.Implementations;

/// <summary>
/// Provides SQL Server data access for notification preference records.
/// </summary>
internal class NotificationPreferenceDataAccess : INotificationPreferenceDataAccess
{
    private AppDbContext appDbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationPreferenceDataAccess"/> class.
    /// </summary>
    /// <param name="appDbContext">The database context used for executing queries.</param>
    public NotificationPreferenceDataAccess(AppDbContext appDbContext)
    {
        this.appDbContext = appDbContext;
    }

    /// <inheritdoc />
    public bool Create(int userId, string category)
    {
        try
        {
            string insertQuery = @"INSERT INTO NotificationPreference (UserId, Category, PushEnabled, EmailEnabled, SmsEnabled)
                                        VALUES
                                        (@p0, @p1, 0, 0, 0);
                                    ";

            int rows = this.appDbContext.ExecuteNonQuery(insertQuery, new object[] { userId, category });

            return rows > 0;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <inheritdoc />
    /// <inheritdoc />
    public List<NotificationPreference> FindByUserId(int userId)
    {
        string selectQuery = @"SELECT * FROM NotificationPreference WHERE UserId = @p0";

        return this.appDbContext.ExecuteQuery(selectQuery, new object[] { userId }, reader =>
        {
            var notificationPreferences = new List<NotificationPreference>();
            while (reader.Read())
            {
                notificationPreferences.Add(new NotificationPreference
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    UserId = Convert.ToInt32(reader["UserId"]),
                    Category = NotificationTypeExtensions.FromString(Convert.ToString(reader["Category"]) ?? string.Empty),
                    PushEnabled = Convert.ToBoolean(reader["PushEnabled"]),
                    EmailEnabled = Convert.ToBoolean(reader["EmailEnabled"]),
                    SmsEnabled = Convert.ToBoolean(reader["SmsEnabled"]),
                    MinAmountThreshold = reader["MinAmountThreshold"] == DBNull.Value ? null : Convert.ToDecimal(reader["MinAmountThreshold"]),
                });
            }

            return notificationPreferences;
        });
    }

    /// <inheritdoc />
    public bool Update(int userId, List<NotificationPreference> preferences)
    {
        try
        {
            string deleteQuery = @"DELETE FROM NotificationPreference WHERE userId = @p0";
            this.appDbContext.ExecuteNonQuery(deleteQuery, new object[] { userId });

            string insertQuery = @"INSERT INTO NotificationPreference (UserId, Category, PushEnabled, EmailEnabled, SmsEnabled, MinAmountThreshold)
                                        VALUES
                                    (@p0, @p1, @p2, @p3, @p4, @p5);
                                ";

            foreach (NotificationPreference preference in preferences)
            {
                this.appDbContext.ExecuteNonQuery(insertQuery, new object[]
                {
                    preference.UserId,
                    NotificationTypeExtensions.ToDisplayName(preference.Category),
                    preference.PushEnabled,
                    preference.EmailEnabled,
                    preference.SmsEnabled,
                    preference.MinAmountThreshold!
                });
            }

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}