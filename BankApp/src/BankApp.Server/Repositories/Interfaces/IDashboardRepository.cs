using BankApp.Contracts.Entities;

namespace BankApp.Server.Repositories.Interfaces;

/// <summary>
/// Defines repository operations for retrieving dashboard data.
/// </summary>
public interface IDashboardRepository
{
    /// <summary>
    /// Gets all accounts belonging to the specified user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <returns>A list of accounts owned by the user.</returns>
    List<Account> GetAccountsByUser(int userId);

    /// <summary>
    /// Gets all cards belonging to the specified user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <returns>A list of cards owned by the user.</returns>
    List<Card> GetCardsByUser(int userId);

    /// <summary>
    /// Gets the most recent transactions for the specified account.
    /// </summary>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="limit">The maximum number of transactions to return. Defaults to 10.</param>
    /// <returns>A list of recent transactions.</returns>
    List<Transaction> GetRecentTransactions(int accountId, int limit = 10);

    /// <summary>
    /// Gets the number of unread notifications for the specified user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <returns>The count of unread notifications.</returns>
    int GetUnreadNotificationCount(int userId);
}