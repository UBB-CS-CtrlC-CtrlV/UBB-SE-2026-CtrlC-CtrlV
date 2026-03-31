using BankApp.Core.Entities;
namespace BankApp.Infrastructure.DataAccess.Interfaces
{
    public interface IPasswordResetTokenDataAccess
    {
        PasswordResetToken Create(int userId, string tokenHash, DateTime expiresAt);
        PasswordResetToken? FindByToken(string tokenHash);
        void MarkAsUsed(int tokenId);
        void DeleteExpired();
    }
}


