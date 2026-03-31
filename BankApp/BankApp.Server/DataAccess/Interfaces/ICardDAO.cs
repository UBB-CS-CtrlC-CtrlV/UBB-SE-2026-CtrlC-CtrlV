using BankApp.Models.Entities;
namespace BankApp.Server.DataAccess.Interfaces
{
    public interface ICardDAO
    {
        List<Card> FindByUserId(int userId);
        Card? FindById(int id);
    }
}