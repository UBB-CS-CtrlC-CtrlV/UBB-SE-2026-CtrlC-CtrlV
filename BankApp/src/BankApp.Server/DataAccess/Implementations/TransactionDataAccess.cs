using BankApp.Contracts.Entities;
using BankApp.Server.DataAccess.Interfaces;
using Dapper;
using ErrorOr;

namespace BankApp.Server.DataAccess.Implementations;

/// <summary>
/// Provides SQL Server data access for financial transaction records.
/// </summary>
public class TransactionDataAccess : ITransactionDataAccess
{
    private const int DefaultTransactionLimit = 10;

    private readonly AppDbContext db;

    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionDataAccess"/> class.
    /// </summary>
    /// <param name="db">The database context used for executing queries.</param>
    public TransactionDataAccess(AppDbContext db)
    {
        this.db = db;
    }

    /// <inheritdoc />
    public ErrorOr<List<Transaction>> FindRecentByAccountId(int accountId, int limit = DefaultTransactionLimit)
    {
        const string sql = """
            SELECT TOP (@Limit)
                Id, AccountId, CardId, TransactionRef, [Type], Direction, Amount,
                Currency, BalanceAfter, CounterpartyName, CounterpartyIBAN, MerchantName,
                CategoryId, Description, Fee, ExchangeRate, [Status], RelatedEntityType,
                RelatedEntityId, CreatedAt
            FROM [Transaction]
            WHERE AccountId = @AccountId
            ORDER BY CreatedAt DESC
            """;

        return this.db.Query(conn => conn.Query<Transaction>(sql, new { AccountId = accountId, Limit = limit }).AsList());
    }
}
