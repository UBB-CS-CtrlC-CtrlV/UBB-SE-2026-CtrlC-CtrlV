using BankApp.Core.Entities;
namespace BankApp.Infrastructure.Repositories.Interfaces
{
    public interface IDashboardRepository
    {
        List<Account> GetAccountsByUser(int userId);
        List<Card> GetCardsByUser(int userId);
        List<Transaction> GetRecentTransactions(int accountId, int limit = 10);
        int GetUnreadNotificationCount(int userId);
    }
}

