using BankApp.Core.Entities;
namespace BankApp.Infrastructure.DataAccess.Interfaces
{
    public interface IAccountDataAccess
    {
        List<Account> FindByUserId(int userId);
        Account? FindById(int id);
    }
}


