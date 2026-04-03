using System.Data;
using BankApp.Core.Entities;
using BankApp.Infrastructure.DataAccess.Interfaces;

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
        public Account? FindById(int id)
        {
            var query =
                @"SELECT Id, UserId, AccountName, IBAN, Currency, Balance, AccountType, Status, CreatedAt from Account where Id = @p0";
            using var reader = dbContext.ExecuteQuery(query, new object[] { id });
            if (reader.Read())
            {
                return MapToAccount(reader);
            }
            return null;
        }

        /// <inheritdoc />
        public List<Account> FindByUserId(int userId)
        {
            var accounts = new List<Account>();
            var query =
                @"SELECT Id, UserId, AccountName, IBAN, Currency, Balance, AccountType, Status, CreatedAt from Account where UserId = @p0";
            using var reader = dbContext.ExecuteQuery(query, new object[] { userId });
            while (reader.Read())
            {
                accounts.Add(MapToAccount(reader));
            }

            return accounts;
        }

        private Account MapToAccount(IDataReader r)
        {
            return new Account
            {
                Id = r.GetInt32(r.GetOrdinal("Id")),
                UserId = r.GetInt32(r.GetOrdinal("UserId")),
                AccountName = r.IsDBNull(r.GetOrdinal("AccountName")) ? null : r.GetString(r.GetOrdinal("AccountName")),
                IBAN = r.GetString(r.GetOrdinal("IBAN")),
                Currency = r.GetString(r.GetOrdinal("Currency")),
                Balance = r.GetDecimal(r.GetOrdinal("Balance")),
                AccountType = r.GetString(r.GetOrdinal("AccountType")),
                Status = r.GetString(r.GetOrdinal("Status")),
                CreatedAt = r.GetDateTime(r.GetOrdinal("CreatedAt"))
            };
        }
    }
}



