using BankApp.Core.Entities;
using BankApp.Infrastructure.Repositories.Interfaces;
using BankApp.Infrastructure.DataAccess.Interfaces;

namespace BankApp.Infrastructure.Repositories.Implementations
{
	public class DashboardRepository : IDashboardRepository
	{
		private readonly IAccountDAO _accountDAO;
		private readonly ICardDAO _cardDAO;
		private readonly ITransactionDAO _transactionDAO;
		private readonly INotificationDAO _notificationDAO;

		public DashboardRepository(IAccountDAO accountDAO, ICardDAO cardDAO, ITransactionDAO transactionDAO, INotificationDAO notificationDAO)
		{
			_accountDAO = accountDAO;
			_cardDAO = cardDAO;
			_transactionDAO = transactionDAO;
			_notificationDAO = notificationDAO;
		}

		public List<Account> GetAccountsByUser(int userId)
		{
			return _accountDAO.FindByUserId(userId);
		}
		public List<Card> GetCardsByUser(int userId)
		{
			return _cardDAO.FindByUserId(userId);
		}
		public List<Transaction> GetRecentTransactions(int accountId, int limit = 10)
		{
			return _transactionDAO.FindRecentByAccountId(accountId, limit);
		}
		public int GetUnreadNotificationCount(int userId)
		{
			return _notificationDAO.CountUnreadByUserId(userId);
		}
	}
}

