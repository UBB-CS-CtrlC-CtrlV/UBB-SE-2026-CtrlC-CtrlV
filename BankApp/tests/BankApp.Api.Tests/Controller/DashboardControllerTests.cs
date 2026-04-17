// <copyright file="DashboardControllerTests.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankApp.Api.Controllers;
using BankApp.Application.DataTransferObjects.Dashboard;
using BankApp.Application.Services.Dashboard;
using ErrorOr;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace BankApp.Api.Tests.Controller;

/// <summary>
/// Unit tests for <see cref="DashboardController"/> verifying route contracts
/// and protected endpoint behavior.
/// </summary>
[Trait("Category", "Unit")]
public sealed class DashboardControllerTests
{
    private readonly Mock<IDashboardService> dashboardService = MockFactory.CreateDashboardService();

    private DashboardController CreateController(int authenticatedUserId)
    {
        var controller = new DashboardController(this.dashboardService.Object);
        var httpContext = new DefaultHttpContext();
        httpContext.Items["UserId"] = authenticatedUserId;
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        return controller;
    }

    [Fact]
    public void GetDashboard_WhenSuccess_ReturnsOkWithData()
    {
        // Arrange
        var validUserId = 1;
        var response = new DashboardResponse();
        this.dashboardService.Setup(service => service.GetDashboardData(validUserId)).Returns(response);
        var controller = this.CreateController(validUserId);

        // Act
        var result = controller.GetDashboard();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(response);
    }

    [Fact]
    public void GetDashboard_WhenUserNotFound_ReturnsNotFound()
    {
        // Arrange
        var nonExistentUserId = 99;
        this.dashboardService
            .Setup(service => service.GetDashboardData(nonExistentUserId))
            .Returns(Error.NotFound("user_not_found", "User not found."));

        var controller = this.CreateController(nonExistentUserId);

        // Act
        var result = controller.GetDashboard();

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }
}
