using BankApp.Core.Entities;
namespace BankApp.Infrastructure.DataAccess.Interfaces
{
    public interface INotificationPreferenceDataAccess
    {
        bool Create(int userId, string category);
        List<NotificationPreference> FindByUserId(int userId);
        bool Update(int userId, List<NotificationPreference> prefs);
    }
}


