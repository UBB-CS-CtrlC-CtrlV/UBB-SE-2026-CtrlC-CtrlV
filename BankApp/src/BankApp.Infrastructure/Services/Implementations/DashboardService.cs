using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BankApp.Core.Entities;
using BankApp.Infrastructure.Services.Interfaces;
using BankApp.Infrastructure.Repositories.Interfaces;
using BankApp.Core.DTOs.Dashboard;

namespace BankApp.Infrastructure.Services.Implementations
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


