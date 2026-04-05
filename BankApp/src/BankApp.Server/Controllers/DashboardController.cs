// <copyright file="DashboardController.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
using BankApp.Core.DTOs.Dashboard;
using BankApp.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Server.Controllers
{
    /// <summary>
    /// Controller responsible for handling dashboard-related operations.
    /// All endpoints are accessible under the /api/dashboard route.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
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
        /// 404 Not Found if no dashboard data exists for the user,
        /// or 500 Internal Server Error if an unexpected error occurs.
        /// </returns>
        [HttpGet]
        public IActionResult GetDashboard()
        {
            try
            {
                if (!int.TryParse(this.HttpContext.Items["UserId"]?.ToString(), out int userId))
                {
                    return this.Unauthorized(new { error = "User is not authenticated." });
                }

                DashboardResponse dashboardData = this.dashService.GetDashboardData(userId) ?? throw new InvalidOperationException();

                return this.Ok(dashboardData);
            }
            catch (Exception)
            {
                return this.StatusCode(
                    500, new { error = "An error occured while fetching the dashboard data." });
            }
        }
    }
}