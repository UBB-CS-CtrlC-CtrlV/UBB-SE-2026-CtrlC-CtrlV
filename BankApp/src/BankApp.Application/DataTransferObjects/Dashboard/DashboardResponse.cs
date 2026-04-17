// <copyright file="DashboardResponse.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

namespace BankApp.Application.DataTransferObjects.Dashboard;

/// <summary>
/// Represents the response containing dashboard data for a user.
/// </summary>
public class DashboardResponse
{
    /// <summary>
    /// Gets or sets the current user information.
    /// </summary>
    public UserSummaryDataTransferObject? CurrentUser { get; set; }

    /// <summary>
    /// Gets or sets the list of cards belonging to the user.
    /// </summary>
    public List<CardDataTransferObject> Cards { get; set; } = new List<CardDataTransferObject>();

    /// <summary>
    /// Gets or sets the list of recent transactions.
    /// </summary>
    public List<TransactionDataTransferObject> RecentTransactions { get; set; } = new List<TransactionDataTransferObject>();

    /// <summary>
    /// Gets or sets the count of unread notifications.
    /// </summary>
    public int UnreadNotificationCount { get; set; }
}
