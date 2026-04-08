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
                .Select(c => new CardDto
                {
                    Id = c.Id,
                    CardNumber = c.CardNumber,
                    CardholderName = c.CardholderName,
                    CardType = c.CardType,
                    CardBrand = c.CardBrand,
                    ExpiryDate = c.ExpiryDate,
                    Status = c.Status,
                    IsContactlessEnabled = c.IsContactlessEnabled,
                    IsOnlineEnabled = c.IsOnlineEnabled,
                })
                .ToList(),
            RecentTransactions = dashboardRepository.GetRecentTransactions(userId, DefaultRecentTransactionLimit)
                .Select(t => new TransactionDto
                {
                    Id = t.Id,
                    Type = t.Type,
                    Direction = t.Direction,
                    Amount = t.Amount,
                    Currency = t.Currency,
                    Description = t.Description,
                    MerchantName = t.MerchantName,
                    CounterpartyName = t.CounterpartyName,
                    Status = t.Status,
                    CreatedAt = t.CreatedAt,
                })
                .ToList(),
            UnreadNotificationCount = dashboardRepository.GetUnreadNotificationCount(userId),
        };
    }
}
