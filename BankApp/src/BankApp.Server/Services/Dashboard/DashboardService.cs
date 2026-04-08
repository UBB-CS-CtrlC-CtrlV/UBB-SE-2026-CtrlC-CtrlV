// <copyright file="DashboardService.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankApp.Contracts.DTOs.Dashboard;
using BankApp.Contracts.Entities;
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
            CurrentUser = new UserSummaryDto
            {
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Is2FAEnabled = user.Is2FAEnabled,
            },
            Cards = dashboardRepository.GetCardsByUser(userId)
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
            RecentTransactions = dashboardRepository.GetRecentTransactions(userId, DefaultRecentTransactionLimit)
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
            UnreadNotificationCount = dashboardRepository.GetUnreadNotificationCount(userId),
        };
    }
}
