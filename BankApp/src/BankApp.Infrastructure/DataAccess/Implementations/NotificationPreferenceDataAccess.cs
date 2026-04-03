using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using BankApp.Core.Entities;
using BankApp.Infrastructure.DataAccess.Interfaces;
using BankApp.Core.Extensions;
using BankApp.Core.Enums;

namespace BankApp.Infrastructure.DataAccess.Implementations
{
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
        public List<NotificationPreference> FindByUserId(int userId)
        {
            List<NotificationPreference> result = new List<NotificationPreference>();
            string selectQuery = @"SELECT * FROM NotificationPreference WHERE userId = @p0";

            using IDataReader data = this.appDbContext.ExecuteQuery(selectQuery, new object[] { userId });

            while (data.Read())
            {
                NotificationPreference notificationPreference = new NotificationPreference
                {
                    Id = Convert.ToInt32(data["Id"]),
                    UserId = Convert.ToInt32(data["UserId"]),
                    Category = NotificationTypeExtensions.FromString(Convert.ToString(data["Category"]) ?? string.Empty),
                    PushEnabled = Convert.ToBoolean(data["PushEnabled"]),
                    EmailEnabled = Convert.ToBoolean(data["EmailEnabled"]),
                    SmsEnabled = Convert.ToBoolean(data["SmsEnabled"]),
                    MinAmountThreshold = data["MinAmountThreshold"] == DBNull.Value ? null : Convert.ToDecimal(data["MinAmountThreshold"])
                };

                result.Add(notificationPreference);
            }

            return result;
        }

        /// <inheritdoc />
        public bool Update(int userId, List<NotificationPreference> prefs)
        {
            try
            {
                string deleteQuery = @"DELETE FROM NotificationPreference WHERE userId = @p0";
                this.appDbContext.ExecuteNonQuery(deleteQuery, new object[] { userId });

                string insertQuery = @"INSERT INTO NotificationPreference (UserId, Category, PushEnabled, EmailEnabled, SmsEnabled, MinAmountThreshold)
                                        VALUES
                                    (@p0, @p1, @p2, @p3, @p4, @p5);
                                ";

                foreach (NotificationPreference preference in prefs)
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
}



