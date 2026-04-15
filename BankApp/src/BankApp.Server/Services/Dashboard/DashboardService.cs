using BankApp.Contracts.DTOs.Dashboard;
using BankApp.Contracts.Entities;
using BankApp.Server.Repositories.Interfaces;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace BankApp.Server.Services.Dashboard;

/// <summary>
/// Provides aggregated dashboard data for users.
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository dashboardRepository;
    private readonly IUserRepository userRepository;
    private readonly ILogger<DashboardService> logger;
    private const int DefaultRecentTransactionLimit = 10;

    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardService"/> class.
    /// </summary>
    /// <param name="dashboardRepository">The dashboard repository.</param>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="logger">The logger.</param>
    public DashboardService(IDashboardRepository dashboardRepository, IUserRepository userRepository, ILogger<DashboardService> logger)
    {
        this.dashboardRepository = dashboardRepository;
        this.userRepository = userRepository;
        this.logger = logger;
    }

    /// <inheritdoc />
    public ErrorOr<DashboardResponse> GetDashboardData(int userId)
    {
        ErrorOr<User> userResult = userRepository.FindById(userId);
        if (userResult.IsError)
        {
            logger.LogWarning("Dashboard fetch failed: user {UserId} not found.", userId);
            return userResult.FirstError;
        }

        ErrorOr<List<Card>> cardsResult = dashboardRepository.GetCardsByUser(userId);
        ErrorOr<List<Transaction>> transactionsResult = dashboardRepository.GetRecentTransactions(userId, DefaultRecentTransactionLimit);
        ErrorOr<int> notifCountResult = dashboardRepository.GetUnreadNotificationCount(userId);

        if (cardsResult.IsError)
        {
            logger.LogError("Failed to fetch cards for user {UserId}: {Error}", userId, cardsResult.FirstError.Description);
        }

        if (transactionsResult.IsError)
        {
            logger.LogError("Failed to fetch transactions for user {UserId}: {Error}", userId, transactionsResult.FirstError.Description);
        }

        if (notifCountResult.IsError)
        {
            logger.LogError("Failed to fetch notification count for user {UserId}: {Error}", userId, notifCountResult.FirstError.Description);
        }

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
