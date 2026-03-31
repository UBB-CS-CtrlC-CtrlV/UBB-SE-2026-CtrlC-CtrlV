using BankApp.Core.Entities;
namespace BankApp.Infrastructure.DataAccess.Interfaces
{
    public interface ISessionDataAccess
    {
        Session Create(int userId, string token, string? deviceInfo, string? browser, string? ip);
        Session? FindByToken(string token);
        List<Session> FindByUserId(int userId);
        void Revoke(int sessionId);
        void RevokeAll(int userId);
    }
}


