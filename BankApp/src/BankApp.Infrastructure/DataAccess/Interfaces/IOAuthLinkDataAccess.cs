using BankApp.Core.Entities;
namespace BankApp.Infrastructure.DataAccess.Interfaces
{
    public interface IOAuthLinkDataAccess
    {
        OAuthLink? FindByProvider(string provider, string providerUserId);
        List<OAuthLink> FindByUserId(int userId);
        bool Create(int userId, string provider, string providerUserId, string? providerEmail);
        void Delete(int id);
    }
}


