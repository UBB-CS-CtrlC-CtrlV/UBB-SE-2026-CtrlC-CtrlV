using BankApp.Core.Entities;
using BankApp.Core.DTOs.Dashboard;
using BankApp.Infrastructure.Repositories.Interfaces;
using BankApp.Infrastructure.Services.Interfaces;

namespace BankApp.Infrastructure.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository dashboardRepository;
        private readonly IUserRepository userRepository;

        public DashboardService(IDashboardRepository dashboardRepository, IUserRepository userRepository)
        {
            this.dashboardRepository = dashboardRepository;
            this.userRepository = userRepository;
        }

        public DashboardResponse? GetDashboardData(int id)
        {
            User? user = userRepository.FindById(id);

            /// if there is no ID returns null, otherwise returns the dashboard data for the user with the given ID
            if (user == null)
            {
                return null;
            }

            return new DashboardResponse
            {
                CurrentUser = user,
                Cards = dashboardRepository.GetCardsByUser(id),
                RecentTransactions = dashboardRepository.GetRecentTransactions(id, 10),
                UnreadNotificationCount = dashboardRepository.GetUnreadNotificationCount(id)
            };
        }
    }
}


