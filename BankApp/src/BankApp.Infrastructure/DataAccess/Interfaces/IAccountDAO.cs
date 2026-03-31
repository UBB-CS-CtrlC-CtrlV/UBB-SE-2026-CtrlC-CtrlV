using BankApp.Core.Entities;
namespace BankApp.Infrastructure.DataAccess.Interfaces
{
    public interface IAccountDAO
    {
        List<Account> FindByUserId(int userId);
        Account? FindById(int id);
    }
}

