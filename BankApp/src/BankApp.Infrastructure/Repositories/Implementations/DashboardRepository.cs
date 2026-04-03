using BankApp.Core.Entities;
using BankApp.Infrastructure.Repositories.Interfaces;
using BankApp.Infrastructure.DataAccess.Interfaces;

namespace BankApp.Infrastructure.Repositories.Implementations
{
	/// <summary>
	/// Provides repository operations for retrieving dashboard data.
	/// </summary>
	public class DashboardRepository : IDashboardRepository
	{
		private readonly IAccountDataAccess accountDAO;
		private readonly ICardDataAccess cardDAO;
		private readonly ITransactionDataAccess transactionDAO;
		private readonly INotificationDataAccess notificationDAO;

		/// <summary>
		/// Initializes a new instance of the <see cref="DashboardRepository"/> class.
		/// </summary>
		/// <param name="accountDAO">The account data access component.</param>
		/// <param name="cardDAO">The card data access component.</param>
		/// <param name="transactionDAO">The transaction data access component.</param>
		/// <param name="notificationDAO">The notification data access component.</param>
		public DashboardRepository(IAccountDataAccess accountDAO, ICardDataAccess cardDAO, ITransactionDataAccess transactionDAO, INotificationDataAccess notificationDAO)
		{
			this.accountDAO = accountDAO;
			this.cardDAO = cardDAO;
			this.transactionDAO = transactionDAO;
			this.notificationDAO = notificationDAO;
		}

		/// <inheritdoc />
		public List<Account> GetAccountsByUser(int userId)
		{
			return accountDAO.FindByUserId(userId);
		}

		/// <inheritdoc />
		public List<Card> GetCardsByUser(int userId)
		{
			return cardDAO.FindByUserId(userId);
		}

		/// <inheritdoc />
		public List<Transaction> GetRecentTransactions(int accountId, int limit = 10)
		{
			return transactionDAO.FindRecentByAccountId(accountId, limit);
		}

		/// <inheritdoc />
		public int GetUnreadNotificationCount(int userId)
		{
			return notificationDAO.CountUnreadByUserId(userId);
		}
	}
}


