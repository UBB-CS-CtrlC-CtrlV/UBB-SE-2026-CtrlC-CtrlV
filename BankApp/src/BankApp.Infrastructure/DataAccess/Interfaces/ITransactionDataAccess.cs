using BankApp.Domain.Entities;
using ErrorOr;

namespace BankApp.Infrastructure.DataAccess.Interfaces;

/// <summary>
/// Defines data access operations for financial transactions.
/// </summary>
public interface ITransactionDataAccess
{
    /// <summary>The default maximum number of transactions to return.</summary>
    public const int DefaultTransactionLimit = 10;

    /// <summary>Finds the most recent transactions for the specified account.</summary>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="limit">The maximum number of transactions to return.</param>
    /// <returns>A list of recent transactions ordered by creation date descending, or an error if the operation failed.</returns>
    ErrorOr<List<Transaction>> FindRecentByAccountId(int accountId, int limit = DefaultTransactionLimit);
}
