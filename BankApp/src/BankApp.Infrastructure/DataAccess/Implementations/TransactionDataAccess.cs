using BankApp.Domain.Entities;
using BankApp.Infrastructure.DataAccess.Interfaces;
using Dapper;
using ErrorOr;

namespace BankApp.Infrastructure.DataAccess.Implementations;

/// <summary>
/// Provides SQL Server data access for financial transaction records.
/// </summary>
public class TransactionDataAccess : ITransactionDataAccess
{
    private const int DefaultTransactionLimit = 10;

    private readonly AppDatabaseContext databaseContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionDataAccess"/> class.
    /// </summary>
    /// <param name="databaseContext">The database context used for executing queries.</param>
    public TransactionDataAccess(AppDatabaseContext databaseContext)
    {
        this.databaseContext = databaseContext;
    }

    /// <inheritdoc />
    public ErrorOr<List<Transaction>> FindRecentByAccountId(int accountId, int limit = DefaultTransactionLimit)
    {
        const string databaseCommandText = """
            SELECT TOP (@Limit)
                Id, AccountId, CardId, TransactionRef, [Type], Direction, Amount,
                Currency, BalanceAfter, CounterpartyName, CounterpartyIBAN, MerchantName,
                CategoryId, Description, Fee, ExchangeRate, [Status], RelatedEntityType,
                RelatedEntityId, CreatedAt
            FROM [Transaction]
            WHERE AccountId = @AccountId
            ORDER BY CreatedAt DESC
            """;

        return this.databaseContext.Query(connection => connection.Query<Transaction>(databaseCommandText, new { AccountId = accountId, Limit = limit }).AsList());
    }
}
