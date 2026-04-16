// Copyright (c) BankApp. All rights reserved.
// Licensed under the MIT license.

using BankApp.Contracts.DTOs.Dashboard;
using BankApp.Server.Controllers;
using BankApp.Server.Services.Dashboard;
using ErrorOr;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace BankApp.Server.Tests.Unit;

/// <summary>
/// Unit tests for <see cref="DashboardController"/> verifying route contracts
/// and protected endpoint behavior.
/// </summary>
[Trait("Category", "Unit")]
public sealed class DashboardControllerTests
{
    private readonly IDashboardService dashService = SubstituteFactory.CreateDashboardService();

    private DashboardController CreateController(int authenticatedUserId)
    {
        var controller = new DashboardController(this.dashService);
        var httpContext = new DefaultHttpContext();
        httpContext.Items["UserId"] = authenticatedUserId;
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        return controller;
    }

    [Fact]
    public void GetDashboard_WhenSuccess_ReturnsOkWithData()
    {
        // Arrange
        var response = new DashboardResponse();
        this.dashService.GetDashboardData(1).Returns(response);
        var controller = this.CreateController(1);

        // Act
        var result = controller.GetDashboard();

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(response);
    }

    [Fact]
    public void GetDashboard_WhenUserNotFound_ReturnsNotFound()
    {
        // Arrange
        this.dashService.GetDashboardData(99)
            .Returns(Error.NotFound("user_not_found", "User not found."));

        var controller = this.CreateController(99);

        // Act
        var result = controller.GetDashboard();

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }
}
