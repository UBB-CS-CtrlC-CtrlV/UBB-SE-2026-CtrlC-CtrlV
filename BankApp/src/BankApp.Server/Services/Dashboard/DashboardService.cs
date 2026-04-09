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
        var userResult = userRepository.FindById(userId);
        if (userResult.IsError)
        {
            return null;
        }

        var cardsResult = dashboardRepository.GetCardsByUser(userId);
        var transactionsResult = dashboardRepository.GetRecentTransactions(userId, DefaultRecentTransactionLimit);
        var notifCountResult = dashboardRepository.GetUnreadNotificationCount(userId);

        return new DashboardResponse
        {
            CurrentUser = new UserSummaryDto
            {
                FullName = userResult.Value.FullName,
                Email = userResult.Value.Email,
                PhoneNumber = userResult.Value.PhoneNumber,
                Is2FAEnabled = userResult.Value.Is2FAEnabled,
            },
            Cards = cardsResult.IsError ? new List<CardDto>() : cardsResult.Value
                .Select(card => new CardDto
                {
                    Id = card.Id,
                    CardNumber = card.CardNumber,
                    CardholderName = card.CardholderName,
                    CardType = card.CardType,
                    CardBrand = card.CardBrand,
                    ExpiryDate = card.ExpiryDate,
                    Status = card.Status,
                    IsContactlessEnabled = card.IsContactlessEnabled,
                    IsOnlineEnabled = card.IsOnlineEnabled,
                })
                .ToList(),
            RecentTransactions = transactionsResult.IsError ? new List<TransactionDto>() : transactionsResult.Value
                .Select(transaction => new TransactionDto
                {
                    Id = transaction.Id,
                    Type = transaction.Type,
                    Direction = transaction.Direction,
                    Amount = transaction.Amount,
                    Currency = transaction.Currency,
                    Description = transaction.Description,
                    MerchantName = transaction.MerchantName,
                    CounterpartyName = transaction.CounterpartyName,
                    Status = transaction.Status,
                    CreatedAt = transaction.CreatedAt,
                })
                .ToList(),
            UnreadNotificationCount = notifCountResult.IsError ? 0 : notifCountResult.Value,
        };
    }
}
