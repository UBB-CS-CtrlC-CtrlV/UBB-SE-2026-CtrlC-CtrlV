// Copyright (c) BankApp. All rights reserved.
// Licensed under the MIT license.

using BankApp.Contracts.DTOs;
using BankApp.Contracts.DTOs.Auth;
using BankApp.Server.Controllers;
using BankApp.Server.Services.Login;
using BankApp.Server.Services.PasswordRecovery;
using BankApp.Server.Services.Registration;
using ErrorOr;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace BankApp.Server.Tests.Unit;

/// <summary>
/// Unit tests for <see cref="AuthController"/> verifying route contracts,
/// status codes, and public endpoint behavior.
/// </summary>
[Trait("Category", "Unit")]
public sealed class AuthControllerTests
{
    private readonly Mock<ILoginService> loginService = new Mock<ILoginService>();
    private readonly Mock<IRegistrationService> registrationService = new Mock<IRegistrationService>();
    private readonly Mock<IPasswordRecoveryService> passwordRecoveryService = new Mock<IPasswordRecoveryService>();

    private AuthController CreateController()
    {
        var controller = new AuthController(
            this.loginService.Object,
            this.registrationService.Object,
            this.passwordRecoveryService.Object);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext(),
        };

        return controller;
    }

    [Fact]
    public void Login_WhenSuccessWithFullLogin_ReturnsOkWithToken()
    {
        // Arrange
        var request = new LoginRequest { Email = "user@test.com", Password = "Pass123!" };
        this.loginService
            .Setup(s => s.Login(request, It.IsAny<SessionMetadata>()))
            .Returns(new FullLogin(1, "jwt-token"));
        var controller = this.CreateController();

        // Act
        var result = controller.Login(request);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be(200);
    }

    [Fact]
    public void Login_WhenRequires2FA_ReturnsOk()
    {
        // Arrange
        var request = new LoginRequest { Email = "user@test.com", Password = "Pass123!" };
        this.loginService
            .Setup(s => s.Login(request, It.IsAny<SessionMetadata>()))
            .Returns(new RequiresTwoFactor(1));
        var controller = this.CreateController();

        // Act
        var result = controller.Login(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void Login_WhenInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest { Email = "user@test.com", Password = "wrong" };
        this.loginService
            .Setup(s => s.Login(request, It.IsAny<SessionMetadata>()))
            .Returns(Error.Unauthorized("invalid_credentials", "Invalid credentials."));
        var controller = this.CreateController();

        // Act
        var result = controller.Login(request);

        // Assert
        var unauthorized = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthorized.StatusCode.Should().Be(401);
    }

    [Fact]
    public void Register_WhenSuccess_ReturnsNoContent()
    {
        // Arrange
        var request = new RegisterRequest { Email = "new@test.com", Password = "Pass123!", FullName = "Test User" };
        this.registrationService.Setup(s => s.Register(request)).Returns(Result.Success);
        var controller = this.CreateController();

        // Act
        var result = controller.Register(request);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public void Register_WhenConflict_ReturnsConflict()
    {
        // Arrange
        var request = new RegisterRequest { Email = "dup@test.com", Password = "Pass123!", FullName = "Test" };
        this.registrationService
            .Setup(s => s.Register(request))
            .Returns(Error.Conflict("email_registered", "Email already registered."));
        var controller = this.CreateController();

        // Act
        var result = controller.Register(request);

        // Assert
        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public void VerifyOTP_WhenSuccess_ReturnsOk()
    {
        // Arrange
        var request = new VerifyOTPRequest { UserId = 1, OTPCode = "123456" };
        this.loginService
            .Setup(s => s.VerifyOTP(request, It.IsAny<SessionMetadata>()))
            .Returns(new FullLogin(1, "jwt-token"));
        var controller = this.CreateController();

        // Act
        var result = controller.VerifyOTP(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void VerifyOTP_WhenInvalidOTP_ReturnsUnauthorized()
    {
        // Arrange
        var request = new VerifyOTPRequest { UserId = 1, OTPCode = "000000" };
        this.loginService
            .Setup(s => s.VerifyOTP(request, It.IsAny<SessionMetadata>()))
            .Returns(Error.Unauthorized("invalid_otp", "Invalid OTP."));
        var controller = this.CreateController();

        // Act
        var result = controller.VerifyOTP(request);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public void ForgotPassword_WhenEmailProvided_ReturnsOk()
    {
        // Arrange
        this.passwordRecoveryService
            .Setup(s => s.RequestPasswordReset("user@test.com"))
            .Returns(Result.Success);
        var controller = this.CreateController();

        // Act
        var result = controller.ForgotPassword(new ForgotPasswordRequest { Email = "user@test.com" });

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void ForgotPassword_WhenEmailEmpty_ReturnsBadRequest()
    {
        // Arrange
        var controller = this.CreateController();

        // Act
        var result = controller.ForgotPassword(new ForgotPasswordRequest { Email = string.Empty });

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void ResetPassword_WhenSuccess_ReturnsNoContent()
    {
        // Arrange
        this.passwordRecoveryService
            .Setup(s => s.ResetPassword("valid-token", "NewPass123!"))
            .Returns(Result.Success);
        var controller = this.CreateController();

        // Act
        var result = controller.ResetPassword(new ResetPasswordRequest { Token = "valid-token", NewPassword = "NewPass123!" });

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public void ResetPassword_WhenTokenMissing_ReturnsBadRequest()
    {
        // Arrange
        var controller = this.CreateController();

        // Act
        var result = controller.ResetPassword(new ResetPasswordRequest { Token = string.Empty, NewPassword = "Pass123!" });

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void ResetPassword_WhenWeakPassword_ReturnsBadRequest()
    {
        // Arrange
        var controller = this.CreateController();

        // Act
        var result = controller.ResetPassword(new ResetPasswordRequest { Token = "token", NewPassword = "weak" });

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void Logout_WhenValidToken_ReturnsNoContent()
    {
        // Arrange
        this.loginService.Setup(s => s.Logout("jwt-token")).Returns(Result.Success);
        var controller = this.CreateController();

        // Act
        var result = controller.Logout("Bearer jwt-token");

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public void Logout_WhenNoAuthorizationHeader_ReturnsBadRequest()
    {
        // Arrange
        var controller = this.CreateController();

        // Act
        var result = controller.Logout(string.Empty);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void ResendOTP_AlwaysReturnsOk()
    {
        // Arrange
        this.loginService.Setup(s => s.ResendOTP(1, "email")).Returns(Result.Success);
        var controller = this.CreateController();

        // Act
        var result = controller.ResendOTP(1, "email");

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void VerifyResetToken_WhenValid_ReturnsNoContent()
    {
        // Arrange
        this.passwordRecoveryService
            .Setup(s => s.VerifyResetToken("valid-token"))
            .Returns(Result.Success);
        var controller = this.CreateController();

        // Act
        var result = controller.VerifyResetToken(new VerifyTokenDto { Token = "valid-token" });

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public void VerifyResetToken_WhenTokenEmpty_ReturnsBadRequest()
    {
        // Arrange
        var controller = this.CreateController();

        // Act
        var result = controller.VerifyResetToken(new VerifyTokenDto { Token = string.Empty });

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task OAuthLogin_WhenSuccess_ReturnsOk()
    {
        // Arrange
        var request = new OAuthLoginRequest { Provider = "Google", ProviderToken = "google-token" };
        this.loginService
            .Setup(s => s.OAuthLoginAsync(request, It.IsAny<SessionMetadata>()))
            .ReturnsAsync(new FullLogin(1, "jwt-token"));
        var controller = this.CreateController();

        // Act
        var result = await controller.OAuthLogin(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task OAuthLogin_WhenProviderMissing_ReturnsBadRequest()
    {
        // Arrange
        var request = new OAuthLoginRequest { Provider = string.Empty, ProviderToken = "token" };
        var controller = this.CreateController();

        // Act
        var result = await controller.OAuthLogin(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task OAuthLogin_WhenAccountLocked_ReturnsForbidden()
    {
        // Arrange
        var request = new OAuthLoginRequest { Provider = "Google", ProviderToken = "google-token" };
        this.loginService
            .Setup(s => s.OAuthLoginAsync(request, It.IsAny<SessionMetadata>()))
            .ReturnsAsync(Error.Forbidden("account_locked", "Account is locked."));
        var controller = this.CreateController();

        // Act
        var result = await controller.OAuthLogin(request);

        // Assert
        var obj = result.Should().BeOfType<ObjectResult>().Subject;
        obj.StatusCode.Should().Be(403);
    }

    [Fact]
    public void ResetPassword_WhenServiceFails_ReturnsMappedError()
    {
        // Arrange
        this.passwordRecoveryService
            .Setup(s => s.ResetPassword("bad-token", "NewPass123!"))
            .Returns(Error.Validation("token_expired", "Token has expired."));
        var controller = this.CreateController();

        // Act
        var result = controller.ResetPassword(new ResetPasswordRequest { Token = "bad-token", NewPassword = "NewPass123!" });

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void Login_WhenAccountLocked_ReturnsForbidden()
    {
        // Arrange
        var request = new LoginRequest { Email = "user@test.com", Password = "Pass123!" };
        this.loginService
            .Setup(s => s.Login(request, It.IsAny<SessionMetadata>()))
            .Returns(Error.Forbidden("account_locked", "Account is locked."));
        var controller = this.CreateController();

        // Act
        var result = controller.Login(request);

        // Assert
        var obj = result.Should().BeOfType<ObjectResult>().Subject;
        obj.StatusCode.Should().Be(403);
    }
}
