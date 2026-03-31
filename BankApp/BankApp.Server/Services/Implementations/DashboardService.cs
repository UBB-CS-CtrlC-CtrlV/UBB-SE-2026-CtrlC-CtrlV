using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BankApp.Models.Entities;
using BankApp.Server.Services.Interfaces;
using BankApp.Server.Repositories.Interfaces;
using BankApp.Models.DTOs.Dashboard;

namespace BankApp.Server.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _dashboardRepository;
        private readonly IUserRepository _userRepository;

        public DashboardService(IDashboardRepository dashboardRepository, IUserRepository userRepository)
        {
            _dashboardRepository = dashboardRepository;
            _userRepository = userRepository;
        }

        public DashboardResponse GetDashboardData(int id)
        {
            User user = _userRepository.FindById(id);
            /// if there is no ID returns null, otherwise returns the dashboard data for the user with the given ID
            if (user == null)
            {
                return null;
            }
            return new DashboardResponse { 
                CurrentUser = user,
                Cards = _dashboardRepository.GetCardsByUser(id),
                RecentTransactions = _dashboardRepository.GetRecentTransactions(id, 10),
                UnreadNotificationCount = _dashboardRepository.GetUnreadNotificationCount(id)
                };
        }
    }
}
