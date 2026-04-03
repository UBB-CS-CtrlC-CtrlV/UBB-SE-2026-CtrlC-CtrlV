using BankApp.Core.Entities;
using BankApp.Infrastructure.Repositories.Interfaces;
using BankApp.Infrastructure.DataAccess.Interfaces;

namespace BankApp.Infrastructure.Repositories.Implementations
{
	public class DashboardRepository : IDashboardRepository
	{
		private readonly IAccountDataAccess accountDAO;
		private readonly ICardDataAccess cardDAO;
		private readonly ITransactionDataAccess transactionDAO;
		private readonly INotificationDataAccess notificationDAO;

		public DashboardRepository(IAccountDataAccess accountDAO, ICardDataAccess cardDAO, ITransactionDataAccess transactionDAO, INotificationDataAccess notificationDAO)
		{
			this.accountDAO = accountDAO;
			this.cardDAO = cardDAO;
			this.transactionDAO = transactionDAO;
			this.notificationDAO = notificationDAO;
		}

		public List<Account> GetAccountsByUser(int userId)
		{
			return accountDAO.FindByUserId(userId);
		}
		public List<Card> GetCardsByUser(int userId)
		{
			return cardDAO.FindByUserId(userId);
		}
		public List<Transaction> GetRecentTransactions(int accountId, int limit = 10)
		{
			return transactionDAO.FindRecentByAccountId(accountId, limit);
		}
		public int GetUnreadNotificationCount(int userId)
		{
			return notificationDAO.CountUnreadByUserId(userId);
		}
	}
}


