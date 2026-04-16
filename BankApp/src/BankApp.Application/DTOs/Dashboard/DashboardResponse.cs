// <copyright file="DashboardResponse.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

namespace BankApp.Application.DTOs.Dashboard;

/// <summary>
/// Represents the response containing dashboard data for a user.
/// </summary>
public class DashboardResponse
{
    /// <summary>
    /// Gets or sets the current user information.
    /// </summary>
    public UserSummaryDto? CurrentUser { get; set; }

    /// <summary>
    /// Gets or sets the list of cards belonging to the user.
    /// </summary>
    public List<CardDto> Cards { get; set; } = new List<CardDto>();

    /// <summary>
    /// Gets or sets the list of recent transactions.
    /// </summary>
    public List<TransactionDto> RecentTransactions { get; set; } = new List<TransactionDto>();

    /// <summary>
    /// Gets or sets the count of unread notifications.
    /// </summary>
    public int UnreadNotificationCount { get; set; }
}
