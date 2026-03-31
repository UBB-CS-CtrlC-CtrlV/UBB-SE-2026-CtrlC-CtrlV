using BankApp.Core.DTOs.Dashboard;
namespace BankApp.Infrastructure.Services.Interfaces
{
    public interface IDashboardService
    {
        DashboardResponse GetDashboardData(int userId);
    }
}

