// <copyright file="ProfileControllerTests.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankApp.Api.Controllers;
using BankApp.Application.DataTransferObjects.Profile;
using BankApp.Application.Services.Profile;
using BankApp.Domain.Enums;
using ErrorOr;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace BankApp.Api.Tests.Controller;

/// <summary>
/// Unit tests for <see cref="ProfileController"/> verifying route contracts
/// and protected endpoint behavior.
/// </summary>
[Trait("Category", "Unit")]
public sealed class ProfileControllerTests
{
    private readonly Mock<IProfileService> profileService = MockFactory.CreateProfileService();

    /// <summary>
    /// Verifies the GetProfile_WhenSuccess_ReturnsOk scenario.
    /// </summary>
    [Fact]
    public void GetProfile_WhenSuccess_ReturnsOk()
    {
        // Arrange
        var info = new ProfileInfo();
        this.profileService.Setup(service => service.GetProfile(1)).Returns(info);
        ProfileController controller = this.CreateController(1);

        // Act
        IActionResult result = controller.GetProfile();

        // Assert
        OkObjectResult? ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(info);
    }

    /// <summary>
    /// Verifies the GetProfile_WhenUserNotFound_ReturnsNotFound scenario.
    /// </summary>
    [Fact]
    public void GetProfile_WhenUserNotFound_ReturnsNotFound()
    {
        // Arrange
        this.profileService
            .Setup(service => service.GetProfile(99))
            .Returns(Error.NotFound("not_found", "User not found."));

        ProfileController controller = this.CreateController(99);

        // Act
        IActionResult result = controller.GetProfile();

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    /// <summary>
    /// Verifies the UpdateProfile_WhenSuccess_ReturnsNoContent scenario.
    /// </summary>
    [Fact]
    public void UpdateProfile_WhenSuccess_ReturnsNoContent()
    {
        // Arrange
        var request = new UpdateProfileRequest();
        this.profileService.Setup(service => service.UpdatePersonalInfo(request)).Returns(Result.Success);
        ProfileController controller = this.CreateController(1);

        // Act
        IActionResult result = controller.UpdateProfile(request);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    /// <summary>
    /// Verifies the ChangePassword_WhenSuccess_ReturnsNoContent scenario.
    /// </summary>
    [Fact]
    public void ChangePassword_WhenSuccess_ReturnsNoContent()
    {
        // Arrange
        var request = new ChangePasswordRequest();
        this.profileService.Setup(service => service.ChangePassword(request)).Returns(Result.Success);
        ProfileController controller = this.CreateController(1);

        // Act
        IActionResult result = controller.ChangePassword(request);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    /// <summary>
    /// Verifies the VerifyPassword_WhenCorrect_ReturnsOkWithTrue scenario.
    /// </summary>
    [Fact]
    public void VerifyPassword_WhenCorrect_ReturnsOkWithTrue()
    {
        // Arrange
        this.profileService.Setup(service => service.VerifyPassword(1, "correct")).Returns(true);
        ProfileController controller = this.CreateController(1);

        // Act
        IActionResult result = controller.VerifyPassword("correct");

        // Assert
        OkObjectResult? ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(true);
    }

    /// <summary>
    /// Verifies the GetSessions_WhenSuccess_ReturnsOk scenario.
    /// </summary>
    [Fact]
    public void GetSessions_WhenSuccess_ReturnsOk()
    {
        // Arrange
        var sessions = new List<SessionDataTransferObject>();
        this.profileService.Setup(service => service.GetActiveSessions(1)).Returns(sessions);
        ProfileController controller = this.CreateController(1);

        // Act
        IActionResult result = controller.GetSessions();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    /// <summary>
    /// Verifies the RevokeSession_WhenSuccess_ReturnsNoContent scenario.
    /// </summary>
    [Fact]
    public void RevokeSession_WhenSuccess_ReturnsNoContent()
    {
        // Arrange
        this.profileService.Setup(service => service.RevokeSession(1, 5)).Returns(Result.Success);
        ProfileController controller = this.CreateController(1);

        // Act
        IActionResult result = controller.RevokeSession(5);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    /// <summary>
    /// Verifies the GetOAuthLinks_WhenSuccess_ReturnsOk scenario.
    /// </summary>
    [Fact]
    public void GetOAuthLinks_WhenSuccess_ReturnsOk()
    {
        // Arrange
        var links = new List<OAuthLinkDataTransferObject>();
        this.profileService.Setup(service => service.GetOAuthLinks(1)).Returns(links);
        ProfileController controller = this.CreateController(1);

        // Act
        IActionResult result = controller.GetOAuthLinks();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    /// <summary>
    /// Verifies the GetNotificationPreferences_WhenSuccess_ReturnsOk scenario.
    /// </summary>
    [Fact]
    public void GetNotificationPreferences_WhenSuccess_ReturnsOk()
    {
        // Arrange
        var preferences = new List<NotificationPreferenceDataTransferObject>();
        this.profileService.Setup(service => service.GetNotificationPreferences(1)).Returns(preferences);
        ProfileController controller = this.CreateController(1);

        // Act
        IActionResult result = controller.GetNotificationPreferences();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    /// <summary>
    /// Verifies the UpdateNotificationPreferences_WhenSuccess_ReturnsNoContent scenario.
    /// </summary>
    [Fact]
    public void UpdateNotificationPreferences_WhenSuccess_ReturnsNoContent()
    {
        // Arrange
        var preferences = new List<NotificationPreferenceDataTransferObject>();
        this.profileService.Setup(service => service.UpdateNotificationPreferences(1, preferences)).Returns(Result.Success);
        ProfileController controller = this.CreateController(1);

        // Act
        IActionResult result = controller.UpdateNotificationPreferences(preferences);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    /// <summary>
    /// Verifies the Enable2FA_WhenSuccess_ReturnsNoContent scenario.
    /// </summary>
    [Fact]
    public void Enable2FA_WhenSuccess_ReturnsNoContent()
    {
        // Arrange
        this.profileService.Setup(service => service.Enable2FactorAuthentication(1, TwoFactorMethod.Email)).Returns(Result.Success);
        ProfileController controller = this.CreateController(1);

        // Act
        IActionResult result = controller.Enable2FA(new Enable2FactorAuthentificationRequest { Method = TwoFactorMethod.Email });

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    /// <summary>
    /// Verifies the Disable2FA_WhenSuccess_ReturnsNoContent scenario.
    /// </summary>
    [Fact]
    public void Disable2FA_WhenSuccess_ReturnsNoContent()
    {
        // Arrange
        this.profileService.Setup(service => service.Disable2FactorAuthentication(1)).Returns(Result.Success);
        ProfileController controller = this.CreateController(1);

        // Act
        IActionResult result = controller.Disable2FA();

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    private ProfileController CreateController(int authenticatedUserId)
    {
        var controller = new ProfileController(this.profileService.Object);
        var httpContext = new DefaultHttpContext();
        httpContext.Items["UserId"] = authenticatedUserId;
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        return controller;
    }
}
