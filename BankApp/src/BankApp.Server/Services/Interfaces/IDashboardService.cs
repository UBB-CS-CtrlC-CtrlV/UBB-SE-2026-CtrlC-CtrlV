using BankApp.Core.DTOs.Dashboard;
namespace BankApp.Infrastructure.Services.Interfaces
{
    /// <summary>
    /// Defines operations for retrieving aggregated dashboard data.
    /// </summary>
    public interface IDashboardService
    {
        /// <summary>
        /// Gets the dashboard data for the specified user.
        /// </summary>
        /// <param name="userId">The identifier of the user.</param>
        /// <returns>A <see cref="DashboardResponse"/> containing the user's dashboard data, or <see langword="null"/> if the user was not found.</returns>
        DashboardResponse? GetDashboardData(int userId);
    }
}

