using BankApp.Core.Entities;
namespace BankApp.Core.DTOs.Dashboard
{
    public class DashboardResponse
    {
        public User CurrentUser { get; set; }
        public List<Card> Cards { get; set; } = new();
        public List<Transaction> RecentTransactions { get; set; } = new();
        public int UnreadNotificationCount { get; set; }
    }
}
