// <copyright file="DashboardController.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankApp.Contracts.DTOs.Dashboard;
using BankApp.Server.Services.Dashboard;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Server.Controllers;

/// <summary>
/// Controller responsible for handling dashboard-related operations.
/// All endpoints are accessible under the /api/dashboard route.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ApiControllerBase
{
    private readonly IDashboardService dashService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardController"/> class.
    /// </summary>
    /// <param name="dashService">The dashboard service used to handle business logic.</param>
    public DashboardController(IDashboardService dashService)
    {
        this.dashService = dashService;
    }

    /// <summary>
    /// Retrieves the dashboard data for the currently authenticated user.
    /// The user ID is extracted from the HTTP context, set by the authentication middleware.
    /// </summary>
    /// <returns>
    /// 200 OK with a <see cref="DashboardResponse"/> on success,
    /// or 404 Not Found if the user does not exist.
    /// </returns>
    [HttpGet]
    public IActionResult GetDashboard()
    {
        int userId = this.GetAuthenticatedUserId();
        return this.ToActionResult(this.dashService.GetDashboardData(userId), data => this.Ok(data));
    }
}
