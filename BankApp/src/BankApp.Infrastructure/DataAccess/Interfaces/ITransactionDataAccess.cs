using BankApp.Core.Entities;
namespace BankApp.Infrastructure.DataAccess.Interfaces
{
    public interface ITransactionDataAccess
    {
        List<Transaction> FindRecentByAccountId(int accountId, int limit = 10);
    }
}



