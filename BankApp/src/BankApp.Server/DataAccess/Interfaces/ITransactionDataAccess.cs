using BankApp.Core.Entities;
namespace BankApp.Infrastructure.DataAccess.Interfaces
{
    /// <summary>
    /// Defines data access operations for financial transactions.
    /// </summary>
    public interface ITransactionDataAccess
    {
        /// <summary>
        /// Finds the most recent transactions for the specified account.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="limit">The maximum number of transactions to return. Defaults to 10.</param>
        /// <returns>A list of recent transactions ordered by creation date descending.</returns>
        List<Transaction> FindRecentByAccountId(int accountId, int limit = 10);
    }
}



