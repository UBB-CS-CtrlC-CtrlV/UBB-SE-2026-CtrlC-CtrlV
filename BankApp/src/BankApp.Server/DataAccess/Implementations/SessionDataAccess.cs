using BankApp.Core.Entities;
using BankApp.Infrastructure.DataAccess.Interfaces;
using BankApp.Server.DataAccess;

namespace BankApp.Infrastructure.DataAccess
{
    /// <summary>
    /// Provides SQL Server data access for user session records.
    /// </summary>
    public class SessionDataAccess : ISessionDataAccess
    {
        private readonly AppDbContext dbContext;
        private const int SessionExpirationDays = 7;

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionDataAccess"/> class.
        /// </summary>
        /// <param name="dbContext">The database context used for executing queries.</param>
        public SessionDataAccess(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <inheritdoc />
        /// <inheritdoc />
        public Session Create(int userId, string token, string? deviceInfo, string? browser, string? ipAddress)
        {
            var sql = @"INSERT INTO [Session] (UserId, Token, DeviceInfo, Browser, IpAddress, LastActiveAt, ExpiresAt)
                OUTPUT INSERTED.Id, INSERTED.UserId, INSERTED.Token, INSERTED.DeviceInfo,
                       INSERTED.Browser, INSERTED.IpAddress, INSERTED.LastActiveAt,
                       INSERTED.ExpiresAt, INSERTED.IsRevoked, INSERTED.CreatedAt
                VALUES (@p0, @p1, @p2, @p3, @p4, GETUTCDATE(), DATEADD(DAY, @p5, GETUTCDATE()))";

            return this.dbContext.ExecuteQuery(sql, new object[]
            {
        userId,
        token,
        deviceInfo ?? (object)DBNull.Value,
        browser ?? (object)DBNull.Value,
        ipAddress ?? (object)DBNull.Value,
        SessionExpirationDays,
            }, reader =>
            {
                reader.Read();
                return this.MapSession(reader);
            });
        }

        /// <inheritdoc />
        public Session? FindByToken(string token)
        {
            var sql = @"SELECT Id, UserId, Token, DeviceInfo, Browser, IpAddress,
                LastActiveAt, ExpiresAt, IsRevoked, CreatedAt
                FROM [Session] WHERE Token = @p0 AND IsRevoked = 0 AND ExpiresAt > GETUTCDATE()";

            return this.dbContext.ExecuteQuery(sql, new object[] { token }, reader =>
                reader.Read() ? this.MapSession(reader) : null);
        }

        /// <inheritdoc />
        public List<Session> FindByUserId(int userId)
        {
            var sql = @"SELECT Id, UserId, Token, DeviceInfo, Browser, IpAddress,
                LastActiveAt, ExpiresAt, IsRevoked, CreatedAt
                FROM [Session] WHERE UserId = @p0 AND IsRevoked = 0 AND ExpiresAt > GETUTCDATE()";

            return this.dbContext.ExecuteQuery(sql, new object[] { userId }, reader =>
            {
                var sessions = new List<Session>();
                while (reader.Read())
                {
                    sessions.Add(this.MapSession(reader));
                }

                return sessions;
            });
        }

        /// <inheritdoc />
        public void Revoke(int sessionId)
        {
            var sql = "UPDATE [Session] SET IsRevoked = 1 WHERE Id = @p0";
            dbContext.ExecuteNonQuery(sql, new object[] { sessionId });
        }

        /// <inheritdoc />
        public void RevokeAll(int userId)
        {
            var sql = "UPDATE [Session] SET IsRevoked = 1 WHERE UserId = @p0 AND IsRevoked = 0";
            dbContext.ExecuteNonQuery(sql, new object[] { userId });
        }

        private Session MapSession(System.Data.IDataReader reader)
        {
            return new Session
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                Token = reader.GetString(reader.GetOrdinal("Token")),
                DeviceInfo = reader.IsDBNull(reader.GetOrdinal("DeviceInfo")) ? null : reader.GetString(reader.GetOrdinal("DeviceInfo")),
                Browser = reader.IsDBNull(reader.GetOrdinal("Browser")) ? null : reader.GetString(reader.GetOrdinal("Browser")),
                IpAddress = reader.IsDBNull(reader.GetOrdinal("IpAddress")) ? null : reader.GetString(reader.GetOrdinal("IpAddress")),
                LastActiveAt = reader.IsDBNull(reader.GetOrdinal("LastActiveAt")) ? null : reader.GetDateTime(reader.GetOrdinal("LastActiveAt")),
                ExpiresAt = reader.GetDateTime(reader.GetOrdinal("ExpiresAt")),
                IsRevoked = reader.GetBoolean(reader.GetOrdinal("IsRevoked")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };
        }
    }
}


