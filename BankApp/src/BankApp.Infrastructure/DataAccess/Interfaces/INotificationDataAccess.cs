using BankApp.Core.Entities;
namespace BankApp.Infrastructure.DataAccess.Interfaces
{
    public interface INotificationDataAccess
    {
        List<Notification> FindByUserId(int userId);
        int CountUnreadByUserId(int userId);
    }
}


