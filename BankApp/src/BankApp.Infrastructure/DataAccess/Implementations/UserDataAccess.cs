using BankApp.Core.Entities;
using BankApp.Infrastructure.DataAccess.Interfaces;

namespace BankApp.Infrastructure.DataAccess.Implementations
{
    /// <summary>
    /// Provides SQL Server data access for user account records.
    /// </summary>
    public class UserDataAccess : IUserDataAccess
    {
        private readonly AppDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserDataAccess"/> class.
        /// </summary>
        /// <param name="dbContext">The database context used for executing queries.</param>
        public UserDataAccess(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <inheritdoc />
        public User? FindByEmail(string email)
        {
            var sql = @"SELECT Id, Email, PasswordHash, FullName, PhoneNumber, DateOfBirth,
                        [Address], Nationality, PreferredLanguage, Is2FAEnabled, Preferred2FAMethod,
                        IsLocked, LockoutEnd, FailedLoginAttempts, CreatedAt, UpdatedAt
                        FROM [User] WHERE Email = @p0";

            using var reader = dbContext.ExecuteQuery(sql, new object[] { email });
            if (reader.Read())
            {
                return MapUser(reader);
            }
            return null;
        }

        /// <inheritdoc />
        public User? FindById(int id)
        {
            var sql = @"SELECT Id, Email, PasswordHash, FullName, PhoneNumber, DateOfBirth,
                        [Address], Nationality, PreferredLanguage, Is2FAEnabled, Preferred2FAMethod,
                        IsLocked, LockoutEnd, FailedLoginAttempts, CreatedAt, UpdatedAt
                        FROM [User] WHERE Id = @p0";

            using var reader = dbContext.ExecuteQuery(sql, new object[] { id });
            if (reader.Read())
            {
                return MapUser(reader);
            }
            return null;
        }

        /// <inheritdoc />
        public bool Create(User user)
        {
            var sql = @"INSERT INTO [User] (Email, PasswordHash, FullName, PhoneNumber, DateOfBirth,
                        [Address], Nationality, PreferredLanguage, Is2FAEnabled, Preferred2FAMethod)
                        VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9)";

            var rows = dbContext.ExecuteNonQuery(sql, new object[]
            {
                user.Email,
                user.PasswordHash,
                user.FullName,
                user.PhoneNumber ?? (object)DBNull.Value,
                user.DateOfBirth ?? (object)DBNull.Value,
                user.Address ?? (object)DBNull.Value,
                user.Nationality ?? (object)DBNull.Value,
                user.PreferredLanguage,
                user.Is2FAEnabled,
                user.Preferred2FAMethod ?? (object)DBNull.Value
            });
            return rows > 0;
        }

        /// <inheritdoc />
        public bool Update(User user)
        {
            var sql = @"UPDATE [User] SET Email = @p0, FullName = @p1, PhoneNumber = @p2,
                        DateOfBirth = @p3, [Address] = @p4, Nationality = @p5,
                        PreferredLanguage = @p6, Is2FAEnabled = @p7, Preferred2FAMethod = @p8,
                        UpdatedAt = GETUTCDATE()
                        WHERE Id = @p9";

            var rows = dbContext.ExecuteNonQuery(sql, new object[]
            {
                user.Email,
                user.FullName,
                user.PhoneNumber ?? (object)DBNull.Value,
                user.DateOfBirth ?? (object)DBNull.Value,
                user.Address ?? (object)DBNull.Value,
                user.Nationality ?? (object)DBNull.Value,
                user.PreferredLanguage,
                user.Is2FAEnabled,
                user.Preferred2FAMethod ?? (object)DBNull.Value,
                user.Id
            });
            return rows > 0;
        }

        /// <inheritdoc />
        public bool UpdatePassword(int userId, string newPasswordHash)
        {
            var sql = "UPDATE [User] SET PasswordHash = @p0, UpdatedAt = GETUTCDATE() WHERE Id = @p1";
            return dbContext.ExecuteNonQuery(sql, new object[] { newPasswordHash, userId }) > 0;
        }

        /// <inheritdoc />
        public void IncrementFailedAttempts(int userId)
        {
            var sql = "UPDATE [User] SET FailedLoginAttempts = FailedLoginAttempts + 1 WHERE Id = @p0";
            dbContext.ExecuteNonQuery(sql, new object[] { userId });
        }

        /// <inheritdoc />
        public void ResetFailedAttempts(int userId)
        {
            var sql = "UPDATE [User] SET FailedLoginAttempts = 0 WHERE Id = @p0";
            dbContext.ExecuteNonQuery(sql, new object[] { userId });
        }

        /// <inheritdoc />
        public void LockAccount(int userId, DateTime lockoutEnd)
        {
            var sql = "UPDATE [User] SET IsLocked = 1, LockoutEnd = @p0 WHERE Id = @p1";
            dbContext.ExecuteNonQuery(sql, new object[] { lockoutEnd, userId });
        }

        private User MapUser(System.Data.IDataReader reader)
        {
            return new User
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                FullName = reader.GetString(reader.GetOrdinal("FullName")),
                PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber")) ? null : reader.GetString(reader.GetOrdinal("PhoneNumber")),
                DateOfBirth = reader.IsDBNull(reader.GetOrdinal("DateOfBirth")) ? null : reader.GetDateTime(reader.GetOrdinal("DateOfBirth")),
                Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader.GetString(reader.GetOrdinal("Address")),
                Nationality = reader.IsDBNull(reader.GetOrdinal("Nationality")) ? null : reader.GetString(reader.GetOrdinal("Nationality")),
                PreferredLanguage = reader.GetString(reader.GetOrdinal("PreferredLanguage")),
                Is2FAEnabled = reader.GetBoolean(reader.GetOrdinal("Is2FAEnabled")),
                Preferred2FAMethod = reader.IsDBNull(reader.GetOrdinal("Preferred2FAMethod")) ? null : reader.GetString(reader.GetOrdinal("Preferred2FAMethod")),
                IsLocked = reader.GetBoolean(reader.GetOrdinal("IsLocked")),
                LockoutEnd = reader.IsDBNull(reader.GetOrdinal("LockoutEnd")) ? null : reader.GetDateTime(reader.GetOrdinal("LockoutEnd")),
                FailedLoginAttempts = reader.GetInt32(reader.GetOrdinal("FailedLoginAttempts")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
            };
        }
    }
}



