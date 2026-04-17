using BankApp.Domain.Entities;
using BankApp.Infrastructure.DataAccess.Interfaces;
using BankApp.Application.Repositories.Interfaces;
using ErrorOr;

namespace BankApp.Infrastructure.Repositories.Implementations;

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
    public DashboardRepository(IAccountDataAccess accountDataAccess, ICardDataAccess cardDataAccess,
        ITransactionDataAccess transactionDataAccess, INotificationDataAccess notificationDataAccess)
    {
        this.accountDataAccess = accountDataAccess;
        this.cardDataAccess = cardDataAccess;
        this.transactionDataAccess = transactionDataAccess;
        this.notificationDataAccess = notificationDataAccess;
    }

    /// <inheritdoc />
    public ErrorOr<List<Account>> GetAccountsByUser(int userId) => accountDataAccess.FindByUserId(userId);

    /// <inheritdoc />
    public ErrorOr<List<Card>> GetCardsByUser(int userId) => cardDataAccess.FindByUserId(userId);

    /// <inheritdoc />
    public ErrorOr<List<Transaction>> GetRecentTransactions(int accountId, int limit = IDashboardRepository.DefaultRecentTransactionLimit) =>
        transactionDataAccess.FindRecentByAccountId(accountId, limit);

    /// <inheritdoc />
    public ErrorOr<int> GetUnreadNotificationCount(int userId) =>
        notificationDataAccess.CountUnreadByUserId(userId);
}
