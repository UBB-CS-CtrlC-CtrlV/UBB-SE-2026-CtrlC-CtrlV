using BankApp.Contracts.Entities;
using BankApp.Contracts.DTOs.Dashboard;
using BankApp.Server.Repositories.Interfaces;

namespace BankApp.Server.Services.Dashboard;

/// <summary>
/// Provides aggregated dashboard data for users.
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository dashboardRepository;
    private readonly IUserRepository userRepository;
    private const int DefaultRecentTransactionLimit = 10;

    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardService"/> class.
    /// </summary>
    /// <param name="dashboardRepository">The dashboard repository.</param>
    /// <param name="userRepository">The user repository.</param>
    public DashboardService(IDashboardRepository dashboardRepository, IUserRepository userRepository)
    {
        this.dashboardRepository = dashboardRepository;
        this.userRepository = userRepository;
    }

    /// <inheritdoc />
    public DashboardResponse? GetDashboardData(int userId)
    {
        User? user = userRepository.FindById(userId);

        if (user == null)
        {
            return null;
        }

        return new DashboardResponse
        {
            CurrentUser = user,
            Cards = dashboardRepository.GetCardsByUser(userId),
            RecentTransactions = dashboardRepository.GetRecentTransactions(userId, DefaultRecentTransactionLimit),
            UnreadNotificationCount = dashboardRepository.GetUnreadNotificationCount(userId)
        };
    }
}