using BankApp.Models.Entities;
namespace BankApp.Server.DataAccess.Interfaces
{
    public interface ITransactionDAO
    {
        List<Transaction> FindRecentByAccountId(int accountId, int limit = 10);
    }
}
