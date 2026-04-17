// <copyright file="AuthControllerTests.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankApp.Api.Controllers;
using BankApp.Application.DataTransferObjects.Auth;
using BankApp.Application.Services.Login;
using BankApp.Application.Services.PasswordRecovery;
using BankApp.Application.Services.Registration;
using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Api.Tests.Controller;

/// <summary>
/// Unit tests for <see cref="AuthController"/> verifying route contracts,
/// status codes, and public endpoint behavior.
/// </summary>
[Trait("Category", "Unit")]
public sealed class AuthControllerTests
{
    private readonly Mock<ILoginService> loginService = MockFactory.CreateLoginService();
    private readonly Mock<IRegistrationService> registrationService = MockFactory.CreateRegistrationService();

    private readonly Mock<IPasswordRecoveryService> passwordRecoveryService =
        MockFactory.CreatePasswordRecoveryService();

    private AuthController CreateController()
    {
        var controller = new AuthController(this.loginService.Object,
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
            .Setup(service => service.Login(request, It.IsAny<SessionMetadata?>()))
            .Returns((ErrorOr<LoginSuccess>)new FullLogin(1, "jwt-token"));
        AuthController controller = this.CreateController();

        // Act
        IActionResult result = controller.Login(request);

        // Assert
        OkObjectResult? ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be(200);
    }

    [Fact]
    public void Login_WhenRequires2FA_ReturnsOk()
    {
        // Arrange
        var request = new LoginRequest { Email = "user@test.com", Password = "Pass123!" };
        this.loginService
            .Setup(service => service.Login(request, It.IsAny<SessionMetadata?>()))
            .Returns((ErrorOr<LoginSuccess>)new RequiresTwoFactor(1));
        AuthController controller = this.CreateController();

        // Act
        IActionResult result = controller.Login(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void Login_WhenInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest { Email = "user@test.com", Password = "wrong" };
        this.loginService
            .Setup(service => service.Login(request, It.IsAny<SessionMetadata?>()))
            .Returns(Error.Unauthorized("invalid_credentials", "Invalid credentials."));
        AuthController controller = this.CreateController();

        // Act
        IActionResult result = controller.Login(request);

        // Assert
        UnauthorizedObjectResult? unauthorized = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthorized.StatusCode.Should().Be(401);
    }

    [Fact]
    public void Login_WhenAccountLocked_ReturnsForbidden()
    {
        // Arrange
        var request = new LoginRequest { Email = "user@test.com", Password = "Pass123!" };
        this.loginService
            .Setup(service => service.Login(request, It.IsAny<SessionMetadata?>()))
            .Returns(Error.Forbidden("account_locked", "Account is locked."));
        AuthController controller = this.CreateController();

        // Act
        IActionResult result = controller.Login(request);

        // Assert
        ObjectResult? obj = result.Should().BeOfType<ObjectResult>().Subject;
        obj.StatusCode.Should().Be(403);
    }

    [Fact]
    public void Login_WhenUnexpectedSuccessType_ReturnsInternalServerError()
    {
        // Arrange
        var request = new LoginRequest { Email = "user@test.com", Password = "Pass123!" };
        this.loginService
            .Setup(service => service.Login(request, It.IsAny<SessionMetadata?>()))
            .Returns((ErrorOr<LoginSuccess>)new UnexpectedLoginSuccess(1));
        AuthController controller = this.CreateController();

        // Act
        IActionResult result = controller.Login(request);

        // Assert
        ObjectResult? obj = result.Should().BeOfType<ObjectResult>().Subject;
        obj.StatusCode.Should().Be(500);
    }

    [Fact]
    public void Register_WhenSuccess_ReturnsNoContent()
    {
        // Arrange
        var request = new RegisterRequest { Email = "new@test.com", Password = "Pass123!", FullName = "Test User" };
        this.registrationService.Setup(service => service.Register(request)).Returns(Result.Success);
        AuthController controller = this.CreateController();

        // Act
        IActionResult result = controller.Register(request);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public void Register_WhenConflict_ReturnsConflict()
    {
        // Arrange
        var request = new RegisterRequest { Email = "dup@test.com", Password = "Pass123!", FullName = "Test" };
        this.registrationService
            .Setup(service => service.Register(request))
            .Returns(Error.Conflict("email_registered", "Email already registered."));
        AuthController controller = this.CreateController();

        // Act
        IActionResult result = controller.Register(request);

        // Assert
        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public void Register_WhenServiceFails_ReturnsInternalServerError()
    {
        // Arrange
        var request = new RegisterRequest { Email = "new@test.com", Password = "Pass123!", FullName = "Test" };
        this.registrationService
            .Setup(service => service.Register(request))
            .Returns(Error.Failure("database_error", "Service unavailable."));
        AuthController controller = this.CreateController();

        // Act
        IActionResult result = controller.Register(request);

        // Assert
        ObjectResult? obj = result.Should().BeOfType<ObjectResult>().Subject;
        obj.StatusCode.Should().Be(500);
    }

    [Fact]
    public void VerifyOTP_WhenSuccess_ReturnsOk()
    {
        // Arrange
        var request = new VerifyOTPRequest { UserId = 1, OTPCode = "123456" };
        this.loginService
            .Setup(service => service.VerifyOTP(request, It.IsAny<SessionMetadata?>()))
            .Returns((ErrorOr<LoginSuccess>)new FullLogin(1, "jwt-token"));
        AuthController controller = this.CreateController();

        // Act
        IActionResult result = controller.VerifyOTP(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void VerifyOTP_WhenInvalidOTP_ReturnsUnauthorized()
    {
        // Arrange
        var request = new VerifyOTPRequest { UserId = 1, OTPCode = "000000" };
        this.loginService
            .Setup(service => service.VerifyOTP(request, It.IsAny<SessionMetadata?>()))
            .Returns(Error.Unauthorized("invalid_otp", "Invalid OTP."));
        AuthController controller = this.CreateController();

        // Act
        IActionResult result = controller.VerifyOTP(request);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public void ForgotPassword_WhenEmailProvided_ReturnsOk()
    {
        // Arrange
        this.passwordRecoveryService
            .Setup(service => service.RequestPasswordReset("user@test.com"))
            .Returns(Result.Success);
        AuthController controller = this.CreateController();

        // Act
        IActionResult result = controller.ForgotPassword(new ForgotPasswordRequest { Email = "user@test.com" });

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void ForgotPassword_WhenEmailEmpty_ReturnsBadRequest()
    {
        // Arrange
        AuthController controller = this.CreateController();

        // Act
        IActionResult result = controller.ForgotPassword(new ForgotPasswordRequest { Email = string.Empty });

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void ResetPassword_WhenSuccess_ReturnsNoContent()
    {
        // Arrange
        this.passwordRecoveryService
            .Setup(service => service.ResetPassword("valid-token", "NewPass123!"))
            .Returns(Result.Success);
        AuthController controller = this.CreateController();

        // Act
        IActionResult result = controller.ResetPassword(new ResetPasswordRequest
            { Token = "valid-token", NewPassword = "NewPass123!" });

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public void ResetPassword_WhenTokenMissing_ReturnsBadRequest()
    {
        // Arrange
        AuthController controller = this.CreateController();

        // Act
        IActionResult result = controller.ResetPassword(new ResetPasswordRequest
            { Token = string.Empty, NewPassword = "Pass123!" });

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void ResetPassword_WhenWeakPassword_ReturnsBadRequest()
    {
        // Arrange
        AuthController controller = this.CreateController();

        // Act
        IActionResult result = controller.ResetPassword(new ResetPasswordRequest
            { Token = "token", NewPassword = "weak" });

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void ResetPassword_WhenServiceFails_ReturnsMappedError()
    {
        // Arrange
        this.passwordRecoveryService
            .Setup(service => service.ResetPassword("bad-token", "NewPass123!"))
            .Returns(Error.Validation("token_expired", "Token has expired."));
        AuthController controller = this.CreateController();

        // Act
        IActionResult result = controller.ResetPassword(new ResetPasswordRequest
            { Token = "bad-token", NewPassword = "NewPass123!" });

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void Logout_WhenValidToken_ReturnsNoContent()
    {
        // Arrange
        this.loginService.Setup(service => service.Logout("jwt-token")).Returns(Result.Success);
        AuthController controller = this.CreateController();

        // Act
        IActionResult result = controller.Logout("Bearer jwt-token");

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public void Logout_WhenNoAuthorizationHeader_ReturnsBadRequest()
    {
        // Arrange
        AuthController controller = this.CreateController();

        // Act
        IActionResult result = controller.Logout(string.Empty);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void ResendOTP_AlwaysReturnsOk()
    {
        // Arrange
        this.loginService.Setup(service => service.ResendOTP(1, "email")).Returns(Result.Success);
        AuthController controller = this.CreateController();

        // Act
        IActionResult result = controller.ResendOTP(1, "email");

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void VerifyResetToken_WhenValid_ReturnsNoContent()
    {
        // Arrange
        this.passwordRecoveryService
            .Setup(service => service.VerifyResetToken("valid-token"))
            .Returns(Result.Success);
        AuthController controller = this.CreateController();

        // Act
        IActionResult result = controller.VerifyResetToken(new VerifyTokenDataTransferObject { Token = "valid-token" });

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public void VerifyResetToken_WhenTokenEmpty_ReturnsBadRequest()
    {
        // Arrange
        AuthController controller = this.CreateController();

        // Act
        IActionResult result = controller.VerifyResetToken(new VerifyTokenDataTransferObject { Token = string.Empty });

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task OAuthLogin_WhenSuccess_ReturnsOk()
    {
        // Arrange
        var request = new OAuthLoginRequest { Provider = "Google", ProviderToken = "google-token" };
        this.loginService
            .Setup(service => service.OAuthLoginAsync(request, It.IsAny<SessionMetadata?>()))
            .ReturnsAsync((ErrorOr<LoginSuccess>)new FullLogin(1, "jwt-token"));
        AuthController controller = this.CreateController();

        // Act
        IActionResult result = await controller.OAuthLogin(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task OAuthLogin_WhenProviderMissing_ReturnsBadRequest()
    {
        // Arrange
        var request = new OAuthLoginRequest { Provider = string.Empty, ProviderToken = "token" };
        AuthController controller = this.CreateController();

        // Act
        IActionResult result = await controller.OAuthLogin(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task OAuthLogin_WhenProviderTokenMissing_ReturnsBadRequest()
    {
        // Arrange
        var request = new OAuthLoginRequest { Provider = "Google", ProviderToken = string.Empty };
        AuthController controller = this.CreateController();

        // Act
        IActionResult result = await controller.OAuthLogin(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task OAuthLogin_WhenAccountLocked_ReturnsForbidden()
    {
        // Arrange
        var request = new OAuthLoginRequest { Provider = "Google", ProviderToken = "google-token" };
        this.loginService
            .Setup(service => service.OAuthLoginAsync(request, It.IsAny<SessionMetadata?>()))
            .ReturnsAsync(Error.Forbidden("account_locked", "Account is locked."));
        AuthController controller = this.CreateController();

        // Act
        IActionResult result = await controller.OAuthLogin(request);

        // Assert
        ObjectResult? obj = result.Should().BeOfType<ObjectResult>().Subject;
        obj.StatusCode.Should().Be(403);
    }

    private sealed class UnexpectedLoginSuccess : LoginSuccess
    {
        public UnexpectedLoginSuccess(int userId)
            : base(userId)
        {
        }
    }
}