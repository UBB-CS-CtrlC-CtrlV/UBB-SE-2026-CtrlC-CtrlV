using BankApp.Core.Entities;
namespace BankApp.Infrastructure.DataAccess.Interfaces
{
    public interface ITransactionDAO
    {
        List<Transaction> FindRecentByAccountId(int accountId, int limit = 10);
    }
}


