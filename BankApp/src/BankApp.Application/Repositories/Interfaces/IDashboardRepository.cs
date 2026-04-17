using BankApp.Domain.Entities;
using ErrorOr;

namespace BankApp.Application.Repositories.Interfaces;

/// <summary>
/// Defines repository operations for retrieving dashboard data.
/// </summary>
public interface IDashboardRepository
{
    /// <summary>The default maximum number of transactions to return.</summary>
    public const int DefaultRecentTransactionLimit = 10;

    /// <summary>Gets all accounts belonging to the specified user.</summary>
    ErrorOr<List<Account>> GetAccountsByUser(int userId);

    /// <summary>Gets all cards belonging to the specified user.</summary>
    ErrorOr<List<Card>> GetCardsByUser(int userId);

    /// <summary>Gets the most recent transactions for the specified account.</summary>
    ErrorOr<List<Transaction>> GetRecentTransactions(int accountId, int limit = DefaultRecentTransactionLimit);

    /// <summary>Gets the number of unread notifications for the specified user.</summary>
    ErrorOr<int> GetUnreadNotificationCount(int userId);
}
