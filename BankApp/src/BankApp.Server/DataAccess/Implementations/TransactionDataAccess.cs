using System.Data;
using BankApp.Core.Entities;
using BankApp.Infrastructure.DataAccess.Interfaces;
using BankApp.Server.DataAccess;

namespace BankApp.Infrastructure.DataAccess.Implementations
{
    /// <summary>
    /// Provides SQL Server data access for financial transaction records.
    /// </summary>
    public class TransactionDataAccess : ITransactionDataAccess
    {
        private readonly AppDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionDataAccess"/> class.
        /// </summary>
        /// <param name="dbContext">The database context used for executing queries.</param>
        public TransactionDataAccess(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <inheritdoc />
        /// <inheritdoc />
        public List<Transaction> FindRecentByAccountId(int accountId, int limit = 10)
        {
            var query = @"SELECT TOP (@p1) * FROM [Transaction]
                  WHERE AccountId = @p0
                  ORDER BY CreatedAt DESC";

            return this.dbContext.ExecuteQuery(query, new object[] { accountId, limit }, reader =>
            {
                var transactions = new List<Transaction>();
                while (reader.Read())
                {
                    transactions.Add(this.MapToTransaction(reader));
                }

                return transactions;
            });
        }

        private Transaction MapToTransaction(IDataReader reader)
        {
            return new Transaction
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                AccountId = reader.GetInt32(reader.GetOrdinal("AccountId")),
                CardId = reader.IsDBNull(reader.GetOrdinal("CardId")) ? null : reader.GetInt32(reader.GetOrdinal("CardId")),
                TransactionRef = reader.GetString(reader.GetOrdinal("TransactionRef")),
                Type = reader.GetString(reader.GetOrdinal("Type")),
                Direction = reader.GetString(reader.GetOrdinal("Direction")),
                Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                Currency = reader.GetString(reader.GetOrdinal("Currency")),
                BalanceAfter = reader.GetDecimal(reader.GetOrdinal("BalanceAfter")),
                CounterpartyName = reader.IsDBNull(reader.GetOrdinal("CounterpartyName")) ? null : reader.GetString(reader.GetOrdinal("CounterpartyName")),
                CounterpartyIBAN = reader.IsDBNull(reader.GetOrdinal("CounterpartyIBAN")) ? null : reader.GetString(reader.GetOrdinal("CounterpartyIBAN")),
                MerchantName = reader.IsDBNull(reader.GetOrdinal("MerchantName")) ? null : reader.GetString(reader.GetOrdinal("MerchantName")),
                CategoryId = reader.IsDBNull(reader.GetOrdinal("CategoryId")) ? null : reader.GetInt32(reader.GetOrdinal("CategoryId")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                Fee = reader.GetDecimal(reader.GetOrdinal("Fee")),
                ExchangeRate = reader.IsDBNull(reader.GetOrdinal("ExchangeRate")) ? null : reader.GetDecimal(reader.GetOrdinal("ExchangeRate")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                RelatedEntityType = reader.IsDBNull(reader.GetOrdinal("RelatedEntityType")) ? null : reader.GetString(reader.GetOrdinal("RelatedEntityType")),
                RelatedEntityId = reader.IsDBNull(reader.GetOrdinal("RelatedEntityId")) ? null : reader.GetInt32(reader.GetOrdinal("RelatedEntityId")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };
        }
    }
}



