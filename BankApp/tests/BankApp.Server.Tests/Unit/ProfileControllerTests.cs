// Copyright (c) BankApp. All rights reserved.
// Licensed under the MIT license.

using BankApp.Contracts.DTOs.Profile;
using BankApp.Contracts.Enums;
using BankApp.Server.Controllers;
using BankApp.Server.Services.Profile;
using ErrorOr;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace BankApp.Server.Tests.Unit;

/// <summary>
/// Unit tests for <see cref="ProfileController"/> verifying route contracts
/// and protected endpoint behavior.
/// </summary>
[Trait("Category", "Unit")]
public sealed class ProfileControllerTests
{
    private readonly Mock<IProfileService> profileService = new Mock<IProfileService>();

    private ProfileController CreateController(int authenticatedUserId)
    {
        var controller = new ProfileController(this.profileService.Object);
        var httpContext = new DefaultHttpContext();
        httpContext.Items["UserId"] = authenticatedUserId;
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        return controller;
    }

    [Fact]
    public void GetProfile_WhenSuccess_ReturnsOk()
    {
        // Arrange
        var info = new ProfileInfo();
        this.profileService.Setup(s => s.GetProfile(1)).Returns(info);
        var controller = this.CreateController(1);

        // Act
        var result = controller.GetProfile();

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(info);
    }

    [Fact]
    public void GetProfile_WhenUserNotFound_ReturnsNotFound()
    {
        // Arrange
        this.profileService
            .Setup(s => s.GetProfile(99))
            .Returns(Error.NotFound("not_found", "User not found."));

        var controller = this.CreateController(99);

        // Act
        var result = controller.GetProfile();

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public void UpdateProfile_WhenSuccess_ReturnsNoContent()
    {
        // Arrange
        var request = new UpdateProfileRequest();
        this.profileService.Setup(s => s.UpdatePersonalInfo(request)).Returns(Result.Success);
        var controller = this.CreateController(1);

        // Act
        var result = controller.UpdateProfile(request);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public void ChangePassword_WhenSuccess_ReturnsNoContent()
    {
        // Arrange
        var request = new ChangePasswordRequest();
        this.profileService.Setup(s => s.ChangePassword(request)).Returns(Result.Success);
        var controller = this.CreateController(1);

        // Act
        var result = controller.ChangePassword(request);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public void VerifyPassword_WhenCorrect_ReturnsOkWithTrue()
    {
        // Arrange
        this.profileService.Setup(s => s.VerifyPassword(1, "correct")).Returns(true);
        var controller = this.CreateController(1);

        // Act
        var result = controller.VerifyPassword("correct");

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(true);
    }

    [Fact]
    public void GetSessions_WhenSuccess_ReturnsOk()
    {
        // Arrange
        var sessions = new List<SessionDto>();
        this.profileService.Setup(s => s.GetActiveSessions(1)).Returns(sessions);
        var controller = this.CreateController(1);

        // Act
        var result = controller.GetSessions();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void RevokeSession_WhenSuccess_ReturnsNoContent()
    {
        // Arrange
        this.profileService.Setup(s => s.RevokeSession(1, 5)).Returns(Result.Success);
        var controller = this.CreateController(1);

        // Act
        var result = controller.RevokeSession(5);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public void GetOAuthLinks_WhenSuccess_ReturnsOk()
    {
        // Arrange
        var links = new List<OAuthLinkDto>();
        this.profileService.Setup(s => s.GetOAuthLinks(1)).Returns(links);
        var controller = this.CreateController(1);

        // Act
        var result = controller.GetOAuthLinks();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void GetNotificationPreferences_WhenSuccess_ReturnsOk()
    {
        // Arrange
        var prefs = new List<NotificationPreferenceDto>();
        this.profileService.Setup(s => s.GetNotificationPreferences(1)).Returns(prefs);
        var controller = this.CreateController(1);

        // Act
        var result = controller.GetNotificationPreferences();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void UpdateNotificationPreferences_WhenSuccess_ReturnsNoContent()
    {
        // Arrange
        var prefs = new List<NotificationPreferenceDto>();
        this.profileService.Setup(s => s.UpdateNotificationPreferences(1, prefs)).Returns(Result.Success);
        var controller = this.CreateController(1);

        // Act
        var result = controller.UpdateNotificationPreferences(prefs);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public void Enable2FA_WhenSuccess_ReturnsNoContent()
    {
        // Arrange
        this.profileService.Setup(s => s.Enable2FA(1, TwoFactorMethod.Email)).Returns(Result.Success);
        var controller = this.CreateController(1);

        // Act
        var result = controller.Enable2FA(new Enable2FARequest { Method = TwoFactorMethod.Email });

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public void Disable2FA_WhenSuccess_ReturnsNoContent()
    {
        // Arrange
        this.profileService.Setup(s => s.Disable2FA(1)).Returns(Result.Success);
        var controller = this.CreateController(1);

        // Act
        var result = controller.Disable2FA();

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }
}
