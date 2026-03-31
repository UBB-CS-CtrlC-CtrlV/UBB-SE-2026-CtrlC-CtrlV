using BankApp.Core.Entities;
namespace BankApp.Infrastructure.DataAccess.Interfaces
{
    public interface ICardDataAccess
    {
        List<Card> FindByUserId(int userId);
        Card? FindById(int id);
    }
}


