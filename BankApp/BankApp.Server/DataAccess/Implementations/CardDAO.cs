using BankApp.Models.Entities;
using BankApp.Server.DataAccess.Interfaces;

namespace BankApp.Server.DataAccess.Implementations
{
    public class CardDAO : ICardDAO
    {
        private readonly AppDbContext _dbContext;
        public CardDAO(AppDbContext dbContext) 
        {
            _dbContext = dbContext;
        }
        public Card? FindById(int id)
        {
            var query = @"SELECT * FROM Card where Id = @p0";
            using var reader = _dbContext.ExecuteQuery(query, new object[] { id });
            if (reader.Read())
                return MapToCard(reader);
            return null;
        }

        public List<Card> FindByUserId(int userId)
        {
            var cards = new List<Card>();
            var query = @"SELECT * FROM Card where UserId = @p0";
            using var reader = _dbContext.ExecuteQuery(query, new object[] { userId });
            while (reader.Read())
            {
                cards.Add(MapToCard(reader));
            }

            return cards;
        }

        private Card MapToCard(System.Data.IDataReader r)
        {
            return new Card
            {
                Id = r.GetInt32(r.GetOrdinal("Id")),
                AccountId = r.GetInt32(r.GetOrdinal("AccountId")),
                UserId = r.GetInt32(r.GetOrdinal("UserId")),
                CardNumber = r.GetString(r.GetOrdinal("CardNumber")),
                CardholderName = r.GetString(r.GetOrdinal("CardholderName")),
                ExpiryDate = r.GetDateTime(r.GetOrdinal("ExpiryDate")),
                CVV = r.GetString(r.GetOrdinal("CVV")),
                CardType = r.GetString(r.GetOrdinal("CardType")),
                CardBrand = r.IsDBNull(r.GetOrdinal("CardBrand")) ? null : r.GetString(r.GetOrdinal("CardBrand")),
                Status = r.GetString(r.GetOrdinal("Status")),
                DailyTransactionLimit = r.IsDBNull(r.GetOrdinal("DailyTransactionLimit")) ? null : r.GetDecimal(r.GetOrdinal("DailyTransactionLimit")),
                MonthlySpendingCap = r.IsDBNull(r.GetOrdinal("MonthlySpendingCap")) ? null : r.GetDecimal(r.GetOrdinal("MonthlySpendingCap")),
                AtmWithdrawalLimit = r.IsDBNull(r.GetOrdinal("AtmWithdrawalLimit")) ? null : r.GetDecimal(r.GetOrdinal("AtmWithdrawalLimit")),
                ContactlessLimit = r.IsDBNull(r.GetOrdinal("ContactlessLimit")) ? null : r.GetDecimal(r.GetOrdinal("ContactlessLimit")),
                IsContactlessEnabled = r.GetBoolean(r.GetOrdinal("IsContactlessEnabled")),
                IsOnlineEnabled = r.GetBoolean(r.GetOrdinal("IsOnlineEnabled")),
                SortOrder = r.GetInt32(r.GetOrdinal("SortOrder")),
                CancelledAt = r.IsDBNull(r.GetOrdinal("CancelledAt")) ? null : r.GetDateTime(r.GetOrdinal("CancelledAt")),
                CreatedAt = r.GetDateTime(r.GetOrdinal("CreatedAt"))
            };
        }
    }
}
