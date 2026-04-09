using BankApp.Contracts.Entities;
using BankApp.Server.DataAccess.Interfaces;
using Dapper;
using ErrorOr;

namespace BankApp.Server.DataAccess.Implementations;

/// <summary>
/// Provides SQL Server data access for payment card records.
/// </summary>
public class CardDataAccess : ICardDataAccess
{
    private const string SelectAllColumns = """
        SELECT Id, AccountId, UserId, CardNumber, CardholderName,
               ExpiryDate, CVV, CardType, CardBrand, Status,
               DailyTransactionLimit, MonthlySpendingCap, AtmWithdrawalLimit,
               ContactlessLimit, IsContactlessEnabled, IsOnlineEnabled,
               SortOrder, CancelledAt, CreatedAt
        FROM Card
        """;

    private readonly AppDbContext db;

    /// <summary>
    /// Initializes a new instance of the <see cref="CardDataAccess"/> class.
    /// </summary>
    /// <param name="db">The database context used for executing queries.</param>
    public CardDataAccess(AppDbContext db)
    {
        this.db = db;
    }

    /// <inheritdoc />
    public ErrorOr<Card> FindById(int id)
    {
        const string query = $"{SelectAllColumns} WHERE Id = @Id";
        return this.db.Query(conn => conn.QueryFirstOrDefault<Card>(query, new { Id = id }))
            .Then(card => card ?? (ErrorOr<Card>)Error.NotFound(description: "Card not found."));
    }

    /// <inheritdoc />
    public ErrorOr<List<Card>> FindByUserId(int userId)
    {
        const string query = $"{SelectAllColumns} WHERE UserId = @UserId";
        return this.db.Query(conn => conn.Query<Card>(query, new { UserId = userId }).AsList());
    }
}
