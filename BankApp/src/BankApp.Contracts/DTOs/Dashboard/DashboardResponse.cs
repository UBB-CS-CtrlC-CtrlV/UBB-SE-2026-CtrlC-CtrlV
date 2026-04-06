using BankApp.Core.Entities;

namespace BankApp.Core.DTOs.Dashboard
{
    /// <summary>
    /// Represents the response containing dashboard data for a user.
    /// </summary>
    public class DashboardResponse
    {
        /// <summary>
        /// Gets or sets the current user information.
        /// </summary>
        public User CurrentUser { get; set; } = null!;

        /// <summary>
        /// Gets or sets the list of cards belonging to the user.
        /// </summary>
        public List<Card> Cards { get; set; } = new ();

        /// <summary>
        /// Gets or sets the list of recent transactions.
        /// </summary>
        public List<Transaction> RecentTransactions { get; set; } = new ();

        /// <summary>
        /// Gets or sets the count of unread notifications.
        /// </summary>
        public int UnreadNotificationCount { get; set; }
    }
}
