using BankApp.Core.Entities;
namespace BankApp.Infrastructure.DataAccess.Interfaces
{
    public interface ICardDAO
    {
        List<Card> FindByUserId(int userId);
        Card? FindById(int id);
    }
}

