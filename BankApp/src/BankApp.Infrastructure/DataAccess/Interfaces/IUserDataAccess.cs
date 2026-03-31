using BankApp.Core.Entities;
namespace BankApp.Infrastructure.DataAccess.Interfaces
{
    public interface IUserDataAccess
    {
        User? FindByEmail(string email);
        User? FindById(int id);
        bool Create(User user);
        bool Update(User user);
        bool UpdatePassword(int userId, string newPasswordHash);
        void IncrementFailedAttempts(int userId);
        void ResetFailedAttempts(int userId);
        void LockAccount(int userId, DateTime lockoutEnd);
    }
}


