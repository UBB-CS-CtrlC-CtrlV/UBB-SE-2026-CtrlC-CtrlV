using BankApp.Contracts.Entities;
using BankApp.Server.DataAccess.Interfaces;
using Dapper;

namespace BankApp.Server.DataAccess.Implementations;

/// <summary>
/// Provides SQL Server data access for payment card records.
/// </summary>
public class CardDataAccess : ICardDataAccess
{
    private const string SelectAllColumns = """                                                                                                                             
      SELECT                                                                                                                                                            
          Id, AccountId, UserId, CardNumber, CardholderName,
          ExpiryDate, CVV, CardType, CardBrand, Status,
          DailyTransactionLimit, MonthlySpendingCap, AtmWithdrawalLimit,
          ContactlessLimit, IsContactlessEnabled, IsOnlineEnabled,
          SortOrder, CancelledAt, CreatedAt
      FROM Card
      """;
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
        const string query = $"{SelectAllColumns} WHERE Id = @Id";

        return this.dbContext.GetConnection().QueryFirstOrDefault<Card>(query, new { Id = id });
    }

    /// <inheritdoc/>
    public List<Card> FindByUserId(int userId)
    {
        const string query = $"{SelectAllColumns} WHERE UserId = @UserId";

        return this.dbContext.GetConnection().Query<Card>(query, new { UserId = userId }).AsList();
    }
}