using System.Data;
using BankApp.Core.Entities;
using BankApp.Infrastructure.DataAccess.Interfaces;
using BankApp.Server.DataAccess;

namespace BankApp.Infrastructure.DataAccess.Implementations
{
    /// <summary>
    /// Provides SQL Server data access for bank account records.
    /// </summary>
    public class AccountDataAccess : IAccountDataAccess
    {
        private readonly AppDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountDataAccess"/> class.
        /// </summary>
        /// <param name="dbContext">The database context used for executing queries.</param>
        public AccountDataAccess(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <inheritdoc />
        /// <inheritdoc />
        public Account? FindById(int id)
        {
            var query =
                @"SELECT Id, UserId, AccountName, IBAN, Currency, Balance, AccountType, Status, CreatedAt FROM Account WHERE Id = @p0";

            return this.dbContext.ExecuteQuery(query, new object[] { id }, reader =>
                reader.Read() ? this.MapToAccount(reader) : null);
        }

        /// <inheritdoc />
        public List<Account> FindByUserId(int userId)
        {
            var query =
                @"SELECT Id, UserId, AccountName, IBAN, Currency, Balance, AccountType, Status, CreatedAt FROM Account WHERE UserId = @p0";

            return this.dbContext.ExecuteQuery(query, new object[] { userId }, reader =>
            {
                var accounts = new List<Account>();
                while (reader.Read())
                {
                    accounts.Add(this.MapToAccount(reader));
                }

                return accounts;
            });
        }

        private Account MapToAccount(IDataReader reader)
        {
            return new Account
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                AccountName = reader.IsDBNull(reader.GetOrdinal("AccountName")) ? null : reader.GetString(reader.GetOrdinal("AccountName")),
                IBAN = reader.GetString(reader.GetOrdinal("IBAN")),
                Currency = reader.GetString(reader.GetOrdinal("Currency")),
                Balance = reader.GetDecimal(reader.GetOrdinal("Balance")),
                AccountType = reader.GetString(reader.GetOrdinal("AccountType")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };
        }
    }
}



