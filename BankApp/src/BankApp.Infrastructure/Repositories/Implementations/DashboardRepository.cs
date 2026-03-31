using BankApp.Core.Entities;
using BankApp.Infrastructure.Repositories.Interfaces;
using BankApp.Infrastructure.DataAccess.Interfaces;

namespace BankApp.Infrastructure.Repositories.Implementations
{
	public class DashboardRepository : IDashboardRepository
	{
		private readonly IAccountDataAccess _accountDAO;
		private readonly ICardDataAccess _cardDAO;
		private readonly ITransactionDataAccess _transactionDAO;
		private readonly INotificationDataAccess _notificationDAO;

		public DashboardRepository(IAccountDataAccess accountDAO, ICardDataAccess cardDAO, ITransactionDataAccess transactionDAO, INotificationDataAccess notificationDAO)
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


