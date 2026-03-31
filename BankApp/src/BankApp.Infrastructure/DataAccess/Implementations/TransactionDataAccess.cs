using BankApp.Core.Entities;
using BankApp.Infrastructure.DataAccess.Interfaces;
using System.Data;

namespace BankApp.Infrastructure.DataAccess.Implementations
{
    public class TransactionDataAccess : ITransactionDataAccess
    {
        private readonly AppDbContext _dbContext;
        public TransactionDataAccess(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public List<Transaction> FindRecentByAccountId(int accountId, int limit = 10)
        {
            var transactions = new List<Transaction>();
            var query = @"SELECT TOP (@p1) * FROM [Transaction] 
                  WHERE AccountId = @p0 
                  ORDER BY CreatedAt DESC";

            using var reader = _dbContext.ExecuteQuery(query, new object[] { accountId, limit });

            while (reader.Read())
            {
                transactions.Add(MapToTransaction(reader));
            }

            return transactions;
        }

        private Transaction MapToTransaction(IDataReader r)
        {
            return new Transaction
            {
                Id = r.GetInt32(r.GetOrdinal("Id")),
                AccountId = r.GetInt32(r.GetOrdinal("AccountId")),
                CardId = r.IsDBNull(r.GetOrdinal("CardId")) ? null : r.GetInt32(r.GetOrdinal("CardId")),
                TransactionRef = r.GetString(r.GetOrdinal("TransactionRef")),
                Type = r.GetString(r.GetOrdinal("Type")),
                Direction = r.GetString(r.GetOrdinal("Direction")),
                Amount = r.GetDecimal(r.GetOrdinal("Amount")),
                Currency = r.GetString(r.GetOrdinal("Currency")),
                BalanceAfter = r.GetDecimal(r.GetOrdinal("BalanceAfter")),
                CounterpartyName = r.IsDBNull(r.GetOrdinal("CounterpartyName")) ? null : r.GetString(r.GetOrdinal("CounterpartyName")),
                CounterpartyIBAN = r.IsDBNull(r.GetOrdinal("CounterpartyIBAN")) ? null : r.GetString(r.GetOrdinal("CounterpartyIBAN")),
                MerchantName = r.IsDBNull(r.GetOrdinal("MerchantName")) ? null : r.GetString(r.GetOrdinal("MerchantName")),
                CategoryId = r.IsDBNull(r.GetOrdinal("CategoryId")) ? null : r.GetInt32(r.GetOrdinal("CategoryId")),
                Description = r.IsDBNull(r.GetOrdinal("Description")) ? null : r.GetString(r.GetOrdinal("Description")),
                Fee = r.GetDecimal(r.GetOrdinal("Fee")),
                ExchangeRate = r.IsDBNull(r.GetOrdinal("ExchangeRate")) ? null : r.GetDecimal(r.GetOrdinal("ExchangeRate")),
                Status = r.GetString(r.GetOrdinal("Status")),
                RelatedEntityType = r.IsDBNull(r.GetOrdinal("RelatedEntityType")) ? null : r.GetString(r.GetOrdinal("RelatedEntityType")),
                RelatedEntityId = r.IsDBNull(r.GetOrdinal("RelatedEntityId")) ? null : r.GetInt32(r.GetOrdinal("RelatedEntityId")),
                CreatedAt = r.GetDateTime(r.GetOrdinal("CreatedAt"))
            };
        }
    }
}



