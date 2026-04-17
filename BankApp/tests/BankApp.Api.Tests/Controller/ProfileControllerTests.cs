// <copyright file="ProfileControllerTests.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankApp.Api.Controllers;
using BankApp.Application.DTOs.Profile;
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
    /// TODO: add docs.
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
    /// TODO: add docs.
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
    /// TODO: add docs.
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
    /// TODO: add docs.
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
    /// TODO: add docs.
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
    /// TODO: add docs.
    /// </summary>
    [Fact]
    public void GetSessions_WhenSuccess_ReturnsOk()
    {
        // Arrange
        var sessions = new List<SessionDto>();
        this.profileService.Setup(service => service.GetActiveSessions(1)).Returns(sessions);
        ProfileController controller = this.CreateController(1);

        // Act
        IActionResult result = controller.GetSessions();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    /// <summary>
    /// TODO: add docs.
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
    /// TODO: add docs.
    /// </summary>
    [Fact]
    public void GetOAuthLinks_WhenSuccess_ReturnsOk()
    {
        // Arrange
        var links = new List<OAuthLinkDto>();
        this.profileService.Setup(service => service.GetOAuthLinks(1)).Returns(links);
        ProfileController controller = this.CreateController(1);

        // Act
        IActionResult result = controller.GetOAuthLinks();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    /// <summary>
    /// TODO: add docs.
    /// </summary>
    [Fact]
    public void GetNotificationPreferences_WhenSuccess_ReturnsOk()
    {
        // Arrange
        var prefs = new List<NotificationPreferenceDto>();
        this.profileService.Setup(service => service.GetNotificationPreferences(1)).Returns(prefs);
        ProfileController controller = this.CreateController(1);

        // Act
        IActionResult result = controller.GetNotificationPreferences();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    /// <summary>
    /// TODO: add docs.
    /// </summary>
    [Fact]
    public void UpdateNotificationPreferences_WhenSuccess_ReturnsNoContent()
    {
        // Arrange
        var prefs = new List<NotificationPreferenceDto>();
        this.profileService.Setup(service => service.UpdateNotificationPreferences(1, prefs)).Returns(Result.Success);
        ProfileController controller = this.CreateController(1);

        // Act
        IActionResult result = controller.UpdateNotificationPreferences(prefs);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    /// <summary>
    /// TODO: add docs.
    /// </summary>
    [Fact]
    public void Enable2FA_WhenSuccess_ReturnsNoContent()
    {
        // Arrange
        this.profileService.Setup(service => service.Enable2FA(1, TwoFactorMethod.Email)).Returns(Result.Success);
        ProfileController controller = this.CreateController(1);

        // Act
        IActionResult result = controller.Enable2FA(new Enable2FARequest { Method = TwoFactorMethod.Email });

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    /// <summary>
    /// TODO: add docs.
    /// </summary>
    [Fact]
    public void Disable2FA_WhenSuccess_ReturnsNoContent()
    {
        // Arrange
        this.profileService.Setup(service => service.Disable2FA(1)).Returns(Result.Success);
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
