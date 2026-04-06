using BankApp.Core.Entities;
using BankApp.Infrastructure.DataAccess.Interfaces;

namespace BankApp.Infrastructure.DataAccess.Implementations
{
    /// <summary>
    /// Provides SQL Server data access for payment card records.
    /// </summary>
    public class CardDataAccess : ICardDataAccess
    {
        private readonly AppDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="CardDataAccess"/> class.
        /// </summary>
        /// <param name="dbContext">The database context used for executing queries.</param>
        public CardDataAccess(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <inheritdoc />
        public Card? FindById(int id)
        {
            var query = @"SELECT * FROM Card where Id = @p0";
            using var reader = dbContext.ExecuteQuery(query, new object[] { id });
            if (reader.Read())
            {
                return MapToCard(reader);
            }
            return null;
        }

        /// <inheritdoc />
        public List<Card> FindByUserId(int userId)
        {
            var cards = new List<Card>();
            var query = @"SELECT * FROM Card where UserId = @p0";
            using var reader = dbContext.ExecuteQuery(query, new object[] { userId });
            while (reader.Read())
            {
                cards.Add(MapToCard(reader));
            }

            return cards;
        }

        private Card MapToCard(System.Data.IDataReader reader)
        {
            return new Card
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                AccountId = reader.GetInt32(reader.GetOrdinal("AccountId")),
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                CardNumber = reader.GetString(reader.GetOrdinal("CardNumber")),
                CardholderName = reader.GetString(reader.GetOrdinal("CardholderName")),
                ExpiryDate = reader.GetDateTime(reader.GetOrdinal("ExpiryDate")),
                CVV = reader.GetString(reader.GetOrdinal("CVV")),
                CardType = reader.GetString(reader.GetOrdinal("CardType")),
                CardBrand = reader.IsDBNull(reader.GetOrdinal("CardBrand")) ? null : reader.GetString(reader.GetOrdinal("CardBrand")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                DailyTransactionLimit = reader.IsDBNull(reader.GetOrdinal("DailyTransactionLimit")) ? null : reader.GetDecimal(reader.GetOrdinal("DailyTransactionLimit")),
                MonthlySpendingCap = reader.IsDBNull(reader.GetOrdinal("MonthlySpendingCap")) ? null : reader.GetDecimal(reader.GetOrdinal("MonthlySpendingCap")),
                AtmWithdrawalLimit = reader.IsDBNull(reader.GetOrdinal("AtmWithdrawalLimit")) ? null : reader.GetDecimal(reader.GetOrdinal("AtmWithdrawalLimit")),
                ContactlessLimit = reader.IsDBNull(reader.GetOrdinal("ContactlessLimit")) ? null : reader.GetDecimal(reader.GetOrdinal("ContactlessLimit")),
                IsContactlessEnabled = reader.GetBoolean(reader.GetOrdinal("IsContactlessEnabled")),
                IsOnlineEnabled = reader.GetBoolean(reader.GetOrdinal("IsOnlineEnabled")),
                SortOrder = reader.GetInt32(reader.GetOrdinal("SortOrder")),
                CancelledAt = reader.IsDBNull(reader.GetOrdinal("CancelledAt")) ? null : reader.GetDateTime(reader.GetOrdinal("CancelledAt")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };
        }
    }
}



