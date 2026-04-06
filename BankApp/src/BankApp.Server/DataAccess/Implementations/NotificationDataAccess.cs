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
        /// <inheritdoc />
        public int CountUnreadByUserId(int userId)
        {
            var query = @"SELECT COUNT(*) FROM Notification WHERE UserId = @p0 AND IsRead = 0";

            return this.dbContext.ExecuteQuery(query, new object[] { userId }, reader =>
                reader.Read() ? reader.GetInt32(0) : 0);
        }

        /// <inheritdoc />
        public List<Notification> FindByUserId(int userId)
        {
            var query = @"SELECT * FROM Notification WHERE UserId = @p0";

            return this.dbContext.ExecuteQuery(query, new object[] { userId }, reader =>
            {
                var notifications = new List<Notification>();
                while (reader.Read())
                {
                    notifications.Add(this.MapToNotification(reader));
                }

                return notifications;
            });
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



