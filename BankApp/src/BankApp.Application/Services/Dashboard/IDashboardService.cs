// <copyright file="IDashboardService.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankApp.Application.DTOs.Dashboard;
using ErrorOr;

namespace BankApp.Application.Services.Dashboard;

/// <summary>
/// Defines operations for aggregating dashboard data for a user.
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Retrieves the full dashboard data for the specified user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <returns>
    /// A <see cref="DashboardResponse"/> containing the user summary, cards, recent transactions,
    /// and unread notification count on success,
    /// or a not-found error if the user does not exist.
    /// Individual sub-queries (cards, transactions, notification count) that fail are logged
    /// and degraded to empty lists or zero rather than propagated as errors.
    /// </returns>
    ErrorOr<DashboardResponse> GetDashboardData(int userId);
}
