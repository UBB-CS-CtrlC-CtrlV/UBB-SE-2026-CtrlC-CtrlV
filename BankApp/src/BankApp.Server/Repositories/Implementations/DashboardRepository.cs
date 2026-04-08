using BankApp.Contracts.Entities;
using BankApp.Server.Repositories.Interfaces;
using BankApp.Server.DataAccess.Interfaces;

namespace BankApp.Server.Repositories.Implementations;

/// <summary>
/// Provides repository operations for retrieving dashboard data.
/// </summary>
public class DashboardRepository : IDashboardRepository
{
    private readonly IAccountDataAccess accountDataAccess;
    private readonly ICardDataAccess cardDataAccess;
    private readonly ITransactionDataAccess transactionDataAccess;
    private readonly INotificationDataAccess notificationDataAccess;

    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardRepository"/> class.
    /// </summary>
    /// <param name="accountDataAccess">The account data access component.</param>
    /// <param name="cardDataAccess">The card data access component.</param>
    /// <param name="transactionDataAccess">The transaction data access component.</param>
    /// <param name="notificationDataAccess">The notification data access component.</param>
    public DashboardRepository(IAccountDataAccess accountDataAccess, ICardDataAccess cardDataAccess, ITransactionDataAccess transactionDataAccess, INotificationDataAccess notificationDataAccess)
    {
        this.accountDataAccess = accountDataAccess;
        this.cardDataAccess = cardDataAccess;
        this.transactionDataAccess = transactionDataAccess;
        this.notificationDataAccess = notificationDataAccess;
    }

    /// <inheritdoc />
    public List<Account> GetAccountsByUser(int userId)
    {
        return accountDataAccess.FindByUserId(userId);
    }

    /// <inheritdoc />
    public List<Card> GetCardsByUser(int userId)
    {
        return cardDataAccess.FindByUserId(userId);
    }

    /// <inheritdoc />
    public List<Transaction> GetRecentTransactions(int accountId, int limit = 10)
    {
        return transactionDataAccess.FindRecentByAccountId(accountId, limit);
    }

    /// <inheritdoc />
    public int GetUnreadNotificationCount(int userId)
    {
        return notificationDataAccess.CountUnreadByUserId(userId);
    }
}