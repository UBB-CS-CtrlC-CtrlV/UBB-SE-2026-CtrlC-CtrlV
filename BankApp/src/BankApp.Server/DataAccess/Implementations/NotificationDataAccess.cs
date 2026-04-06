using BankApp.Core.Entities;
using BankApp.Infrastructure.DataAccess.Interfaces;
using BankApp.Server.DataAccess;

namespace BankApp.Infrastructure.DataAccess.Implementations
{
    /// <summary>
    /// Provides SQL Server data access for notification records.
    /// </summary>
    public class NotificationDataAccess : INotificationDataAccess
    {
        private readonly AppDbContext dbContext;

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
            var query = @"SELECT COUNT(*) FROM Notification WHERE UserId = @p0 and IsRead = 0";
            using var reader = dbContext.ExecuteQuery(query, new object[] { userId });
            if (reader.Read())
            {
                return reader.GetInt32(0);
            }
            return 0;
        }

        /// <inheritdoc />
        public List<Notification> FindByUserId(int userId)
        {
            var notifications = new List<Notification>();
            var query = @"SELECT * FROM Notification where UserId = @p0";
            using var reader = dbContext.ExecuteQuery(query, new object[] { userId });
            while (reader.Read())
            {
                notifications.Add(MapToNotification(reader));
            }

            return notifications;
        }

        private Notification MapToNotification(System.Data.IDataReader reader)
        {
            return new Notification
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                Title = reader.GetString(reader.GetOrdinal("Title")),
                Message = reader.GetString(reader.GetOrdinal("Message")),
                Type = reader.GetString(reader.GetOrdinal("Type")),
                Channel = reader.GetString(reader.GetOrdinal("Channel")),
                IsRead = reader.GetBoolean(reader.GetOrdinal("IsRead")),
                RelatedEntityType = reader.IsDBNull(reader.GetOrdinal("RelatedEntityType"))
                    ? null : reader.GetString(reader.GetOrdinal("RelatedEntityType")),
                RelatedEntityId = reader.IsDBNull(reader.GetOrdinal("RelatedEntityId"))
                    ? null : reader.GetInt32(reader.GetOrdinal("RelatedEntityId")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };
        }
    }
}



