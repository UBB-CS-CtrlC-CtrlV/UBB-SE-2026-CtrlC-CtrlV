using System;
using System.Threading.Tasks;
using ErrorOr;
using BankApp.Contracts.DTOs.Auth;
using BankApp.Contracts.Entities;
using BankApp.Server.Repositories.Interfaces;
using BankApp.Server.Services.Auth;
using BankApp.Server.Services.Notifications;
using BankApp.Server.Services.Security;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace BankApp.Server.Tests.Unit.Services.Auth;

public sealed class AuthServiceTests
{
    private readonly Mock<IAuthRepository> mockAuthRepository = MockFactory.CreateAuthRepository();
    private readonly Mock<IHashService> mockHashService = MockFactory.CreateHashService();
    private readonly Mock<IJwtService> mockJwtService = MockFactory.CreateJwtService();
    private readonly Mock<IOtpService> mockOtpService = MockFactory.CreateOtpService();
    private readonly Mock<IEmailService> mockEmailService = MockFactory.CreateEmailService();
    private readonly AuthService authService;

    public AuthServiceTests()
    {
        this.authService = new AuthService(
            this.mockAuthRepository.Object,
            this.mockHashService.Object,
            this.mockJwtService.Object,
            this.mockOtpService.Object,
            this.mockEmailService.Object,
            NullLogger<AuthService>.Instance);
    }

    [Fact]
    public void Login_WhenEmailIsNotValid_ReturnsError()
    {
        // Arrange
        var request = new LoginRequest { Email = "invalid_email", Password = "invalid_password" };

        // Act
        var result = this.authService.Login(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("invalid_email");
    }

    [Fact]
    public void Login_WhenUserNotFound_ReturnsError()
    {
        // Arrange
        var request = new LoginRequest { Email = "fake@user.com", Password = "fake_password" };
        this.mockAuthRepository.Setup(m => m.FindUserByEmail(request.Email)).Returns((ErrorOr<User>)Error.Failure());

        // Act
        var result = this.authService.Login(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Unauthorized);
        result.FirstError.Code.Should().Be("invalid_credentials");
    }

    [Fact]
    public void Login_WhenAccountIsLocked_ReturnsForbidden()
    {
        // Arrange
        var request = new LoginRequest { Email = "locked@user.com", Password = "password" };
        var user = new User { Id = 1, Email = request.Email, IsLocked = true, LockoutEnd = DateTime.UtcNow.AddMinutes(10) };
        this.mockAuthRepository.Setup(m => m.FindUserByEmail(request.Email)).Returns((ErrorOr<User>)user);

        // Act
        var result = this.authService.Login(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Forbidden);
        result.FirstError.Code.Should().Be("account_locked");
    }

    [Fact]
    public void Login_WhenPasswordVerificationThrowsError_ReturnsError()
    {
        // Arrange
        var request = new LoginRequest { Email = "test@test.com", Password = "ValidPassword1!" };
        var user = new User { Id = 1, Email = request.Email, PasswordHash = "hash", IsLocked = false };
        this.mockAuthRepository.Setup(m => m.FindUserByEmail(request.Email)).Returns((ErrorOr<User>)user);
        this.mockHashService.Setup(m => m.Verify(request.Password, user.PasswordHash)).Returns((ErrorOr<bool>)Error.Failure("verify_failed"));

        // Act
        var result = this.authService.Login(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("verify_failed");
    }

    [Fact]
    public void Login_WhenPasswordIsWrong_IncrementsFailedAttempts()
    {
        // Arrange
        var request = new LoginRequest { Email = "wrongpass@user.com", Password = "WrongPassword1!" };
        var user = new User { Id = 1, Email = request.Email, PasswordHash = "hash", FailedLoginAttempts = 1 };
        this.mockAuthRepository.Setup(m => m.FindUserByEmail(request.Email)).Returns((ErrorOr<User>)user);
        this.mockHashService.Setup(m => m.Verify(request.Password, user.PasswordHash)).Returns((ErrorOr<bool>)false);
        this.mockAuthRepository.Setup(m => m.IncrementFailedAttempts(user.Id)).Returns(Result.Success);

        // Act
        var result = this.authService.Login(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("invalid_credentials");
        this.mockAuthRepository.Verify(m => m.IncrementFailedAttempts(user.Id), Times.Once);
    }

    [Fact]
    public void Login_WhenPasswordIsWrong_AndMaxAttemptsReached_LockAccountFails_ReturnsError()
    {
        // Arrange
        var request = new LoginRequest { Email = "lockme@user.com", Password = "WrongPassword1!" };
        var user = new User { Id = 1, Email = request.Email, PasswordHash = "hash", FailedLoginAttempts = 4 };
        this.mockAuthRepository.Setup(m => m.FindUserByEmail(request.Email)).Returns((ErrorOr<User>)user);
        this.mockHashService.Setup(m => m.Verify(request.Password, user.PasswordHash)).Returns((ErrorOr<bool>)false);
        this.mockAuthRepository.Setup(m => m.IncrementFailedAttempts(user.Id)).Returns(Result.Success);
        this.mockAuthRepository.Setup(m => m.LockAccount(user.Id, It.IsAny<DateTime>())).Returns((ErrorOr<Success>)Error.Failure("lock_failed"));

        // Act
        var result = this.authService.Login(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Unauthorized);
        result.FirstError.Code.Should().Be("invalid_credentials");
    }

    [Fact]
    public void Login_WhenPasswordIsWrong_AndMaxAttemptsReached_LocksAccount()
    {
        // Arrange
        var request = new LoginRequest { Email = "lockme@user.com", Password = "WrongPassword1!" };
        var user = new User { Id = 1, Email = request.Email, PasswordHash = "hash", FailedLoginAttempts = 4 };
        this.mockAuthRepository.Setup(m => m.FindUserByEmail(request.Email)).Returns((ErrorOr<User>)user);
        this.mockHashService.Setup(m => m.Verify(request.Password, user.PasswordHash)).Returns((ErrorOr<bool>)false);
        this.mockAuthRepository.Setup(m => m.IncrementFailedAttempts(user.Id)).Returns(Result.Success);
        this.mockAuthRepository.Setup(m => m.LockAccount(user.Id, It.IsAny<DateTime>())).Returns(Result.Success);

        // Act
        var result = this.authService.Login(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Forbidden);
        this.mockAuthRepository.Verify(m => m.LockAccount(user.Id, It.IsAny<DateTime>()), Times.Once);
        this.mockEmailService.Verify(m => m.SendLockNotification(user.Email), Times.Once);
    }

    [Fact]
    public void Login_When2FAEnabled_AndOtpGenerationFails_ReturnsError()
    {
        // Arrange
        var request = new LoginRequest { Email = "2fa@user.com", Password = "ValidPassword123!" };
        var user = new User { Id = 1, Email = request.Email, PasswordHash = "hash", Is2FAEnabled = true, Preferred2FAMethod = "Email" };
        this.mockAuthRepository.Setup(m => m.FindUserByEmail(request.Email)).Returns((ErrorOr<User>)user);
        this.mockHashService.Setup(m => m.Verify(request.Password, user.PasswordHash)).Returns((ErrorOr<bool>)true);
        this.mockOtpService.Setup(m => m.GenerateTOTP(user.Id)).Returns((ErrorOr<string>)Error.Failure("otp_failed"));

        // Act
        var result = this.authService.Login(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("otp_failed");
    }

    [Fact]
    public void Login_WhenUserHas2FA_ReturnsRequiresTwoFactor()
    {
        // Arrange
        var request = new LoginRequest { Email = "2fa@user.com", Password = "ValidPassword123!" };
        var user = new User { Id = 1, Email = request.Email, PasswordHash = "hash", Is2FAEnabled = true, Preferred2FAMethod = "Email" };
        this.mockAuthRepository.Setup(m => m.FindUserByEmail(request.Email)).Returns((ErrorOr<User>)user);
        this.mockHashService.Setup(m => m.Verify(request.Password, user.PasswordHash)).Returns((ErrorOr<bool>)true);
        this.mockOtpService.Setup(m => m.GenerateTOTP(user.Id)).Returns((ErrorOr<string>)"123456");

        // Act
        var result = this.authService.Login(request);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeOfType<RequiresTwoFactor>();
        this.mockEmailService.Verify(m => m.SendOTPCode(user.Email, "123456"), Times.Once);
    }

    [Fact]
    public void Login_WhenCompleteLogin_AndTokenGenerationFails_ReturnsError()
    {
        // Arrange
        var request = new LoginRequest { Email = "ok@user.com", Password = "ValidPassword123!" };
        var user = new User { Id = 1, Email = request.Email, PasswordHash = "hash", Is2FAEnabled = false };
        this.mockAuthRepository.Setup(m => m.FindUserByEmail(request.Email)).Returns((ErrorOr<User>)user);
        this.mockHashService.Setup(m => m.Verify(request.Password, user.PasswordHash)).Returns((ErrorOr<bool>)true);
        this.mockAuthRepository.Setup(m => m.ResetFailedAttempts(user.Id)).Returns(Result.Success);
        this.mockJwtService.Setup(m => m.GenerateToken(user.Id)).Returns((ErrorOr<string>)Error.Failure("jwt_failed"));

        // Act
        var result = this.authService.Login(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("jwt_failed");
    }

    [Fact]
    public void Login_WhenCompleteLogin_AndSessionCreationFailed_ReturnsError()
    {
        // Arrange
        var request = new LoginRequest { Email = "ok@user.com", Password = "ValidPassword123!" };
        var user = new User { Id = 1, Email = request.Email, PasswordHash = "hash", Is2FAEnabled = false };
        this.mockAuthRepository.Setup(m => m.FindUserByEmail(request.Email)).Returns((ErrorOr<User>)user);
        this.mockHashService.Setup(m => m.Verify(request.Password, user.PasswordHash)).Returns((ErrorOr<bool>)true);
        this.mockAuthRepository.Setup(m => m.ResetFailedAttempts(user.Id)).Returns(Result.Success);
        this.mockJwtService.Setup(m => m.GenerateToken(user.Id)).Returns((ErrorOr<string>)"jwt-token");
        this.mockAuthRepository.Setup(m => m.CreateSession(user.Id, "jwt-token", null, null, null)).Returns((ErrorOr<Session>)Error.Failure("session_failed"));

        // Act
        var result = this.authService.Login(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("session_failed");
    }

    [Fact]
    public void Login_WhenValid_ReturnsFullLogin()
    {
        // Arrange
        var request = new LoginRequest { Email = "ok@user.com", Password = "ValidPassword123!" };
        var user = new User { Id = 1, Email = request.Email, PasswordHash = "hash", Is2FAEnabled = false };
        var token = "jwt-token";
        this.mockAuthRepository.Setup(m => m.FindUserByEmail(request.Email)).Returns((ErrorOr<User>)user);
        this.mockHashService.Setup(m => m.Verify(request.Password, user.PasswordHash)).Returns((ErrorOr<bool>)true);
        this.mockAuthRepository.Setup(m => m.ResetFailedAttempts(user.Id)).Returns(Result.Success);
        this.mockJwtService.Setup(m => m.GenerateToken(user.Id)).Returns((ErrorOr<string>)token);
        this.mockAuthRepository.Setup(m => m.CreateSession(user.Id, token, null, null, null)).Returns((ErrorOr<Session>)new Session());

        // Act
        var result = this.authService.Login(request);

        // Assert
        result.IsError.Should().BeFalse();
        var login = result.Value.Should().BeOfType<FullLogin>().Subject;
        login.UserId.Should().Be(user.Id);
        login.Token.Should().Be(token);
    }

    [Fact]
    public void Register_WhenEmailIsInvalid_ReturnsValidationError()
    {
        // Arrange
        var request = new RegisterRequest { Email = "invalid", Password = "ValidPassword1!", FullName = "Name" };

        // Act
        var result = this.authService.Register(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("invalid_email");
    }

    [Fact]
    public void Register_WhenPasswordIsWeak_ReturnsValidationError()
    {
        // Arrange
        var request = new RegisterRequest { Email = "test@user.com", Password = "weak", FullName = "John Doe" };

        // Act
        var result = this.authService.Register(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("weak_password");
    }

    [Fact]
    public void Register_WhenFullNameIsEmpty_ReturnsValidationError()
    {
        // Arrange
        var request = new RegisterRequest { Email = "test@user.com", Password = "ValidPassword1!", FullName = string.Empty };

        // Act
        var result = this.authService.Register(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("full_name_required");
    }

    [Fact]
    public void Register_WhenEmailAlreadyExists_ReturnsConflict()
    {
        // Arrange
        var request = new RegisterRequest { Email = "existing@user.com", Password = "ValidPassword123!", FullName = "John Doe" };
        this.mockAuthRepository.Setup(m => m.FindUserByEmail(request.Email)).Returns((ErrorOr<User>)new User());

        // Act
        var result = this.authService.Register(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("email_registered");
    }

    [Fact]
    public void Register_WhenHashGenerationFails_ReturnsError()
    {
        // Arrange
        var request = new RegisterRequest { Email = "new@user.com", Password = "ValidPassword123!", FullName = "John Doe" };
        this.mockAuthRepository.Setup(m => m.FindUserByEmail(request.Email)).Returns((ErrorOr<User>)Error.NotFound());
        this.mockHashService.Setup(m => m.GetHash(request.Password)).Returns((ErrorOr<string>)Error.Failure("hash_failed"));

        // Act
        var result = this.authService.Register(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("hash_failed");
    }

    [Fact]
    public void Register_WhenUserCreationFailed_ReturnsError()
    {
        // Arrange
        var request = new RegisterRequest { Email = "new@user.com", Password = "ValidPassword123!", FullName = "John Doe" };
        this.mockAuthRepository.Setup(m => m.FindUserByEmail(request.Email)).Returns((ErrorOr<User>)Error.NotFound());
        this.mockHashService.Setup(m => m.GetHash(request.Password)).Returns((ErrorOr<string>)"hashed_pass");
        this.mockAuthRepository.Setup(m => m.CreateUser(It.IsAny<User>())).Returns((ErrorOr<Success>)Error.Failure("create_failed"));

        // Act
        var result = this.authService.Register(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("user_creation_failed");
    }

    [Fact]
    public void Register_WhenValid_ReturnsSuccess()
    {
        // Arrange
        var request = new RegisterRequest { Email = "new@user.com", Password = "ValidPassword123!", FullName = "John Doe" };
        this.mockAuthRepository.Setup(m => m.FindUserByEmail(request.Email)).Returns((ErrorOr<User>)Error.NotFound());
        this.mockHashService.Setup(m => m.GetHash(request.Password)).Returns((ErrorOr<string>)"hashed_pass");
        this.mockAuthRepository.Setup(m => m.CreateUser(It.IsAny<User>())).Returns(Result.Success);

        // Act
        var result = this.authService.Register(request);

        // Assert
        result.IsError.Should().BeFalse();
        this.mockAuthRepository.Verify(m => m.CreateUser(It.Is<User>(u => u.Email == request.Email && u.FullName == request.FullName)), Times.Once);
    }

    [Fact]
    public async Task OAuthLoginAsync_WhenProviderIsNotGoogle_ReturnsError()
    {
        // Arrange
        var request = new OAuthLoginRequest { Provider = "Facebook", ProviderToken = "token" };

        // Act
        var result = await this.authService.OAuthLoginAsync(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("unsupported_provider");
    }

    [Fact]
    public async Task OAuthLoginAsync_WhenTokenIsInvalid_ReturnsError()
    {
        // Arrange
        var request = new OAuthLoginRequest { Provider = "Google", ProviderToken = "invalid_token" };

        // Act
        var result = await this.authService.OAuthLoginAsync(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("invalid_google_token");
    }

    [Fact]
    public void OAuthRegister_WhenEmailIsInvalid_ReturnsError()
    {
        // Arrange
        var request = new OAuthRegisterRequest { Email = "invalid", Provider = "Google", ProviderToken = "token", FullName = "Name" };

        // Act
        var result = this.authService.OAuthRegister(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("invalid_email");
    }

    [Fact]
    public void OAuthRegister_WhenOAuthLinkExists_ReturnsConflict()
    {
        // Arrange
        var request = new OAuthRegisterRequest { Email = "test@test.com", Provider = "Google", ProviderToken = "token", FullName = "Name" };
        this.mockAuthRepository.Setup(m => m.FindOAuthLink(request.Provider, request.ProviderToken)).Returns((ErrorOr<OAuthLink>)new OAuthLink());

        // Act
        var result = this.authService.OAuthRegister(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("oauth_already_registered");
    }

    [Fact]
    public void OAuthRegister_WhenUserExists_AndCreateLinkFails_ReturnsError()
    {
        // Arrange
        var request = new OAuthRegisterRequest { Email = "test@test.com", Provider = "Google", ProviderToken = "token", FullName = "Name" };
        var user = new User { Id = 1, Email = request.Email };
        this.mockAuthRepository.Setup(m => m.FindOAuthLink(request.Provider, request.ProviderToken)).Returns((ErrorOr<OAuthLink>)Error.NotFound());
        this.mockAuthRepository.Setup(m => m.FindUserByEmail(request.Email)).Returns((ErrorOr<User>)user);
        this.mockAuthRepository.Setup(m => m.CreateOAuthLink(It.IsAny<OAuthLink>())).Returns((ErrorOr<Success>)Error.Failure("link_failed"));

        // Act
        var result = this.authService.OAuthRegister(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("oauth_link_failed");
    }

    [Fact]
    public void OAuthRegister_WhenUserExists_AndCreateLinkSucceeds_ReturnsSuccess()
    {
        // Arrange
        var request = new OAuthRegisterRequest { Email = "test@test.com", Provider = "Google", ProviderToken = "token", FullName = "Name" };
        var user = new User { Id = 1, Email = request.Email };
        this.mockAuthRepository.Setup(m => m.FindOAuthLink(request.Provider, request.ProviderToken)).Returns((ErrorOr<OAuthLink>)Error.NotFound());
        this.mockAuthRepository.Setup(m => m.FindUserByEmail(request.Email)).Returns((ErrorOr<User>)user);
        this.mockAuthRepository.Setup(m => m.CreateOAuthLink(It.IsAny<OAuthLink>())).Returns(Result.Success);

        // Act
        var result = this.authService.OAuthRegister(request);

        // Assert
        result.IsError.Should().BeFalse();
    }

    [Fact]
    public void OAuthRegister_WhenUserDoesNotExist_AndHashGenerationFails_ReturnsError()
    {
        // Arrange
        var request = new OAuthRegisterRequest { Email = "test@test.com", Provider = "Google", ProviderToken = "token", FullName = "Name" };
        this.mockAuthRepository.Setup(m => m.FindOAuthLink(request.Provider, request.ProviderToken)).Returns((ErrorOr<OAuthLink>)Error.NotFound());
        this.mockAuthRepository.Setup(m => m.FindUserByEmail(request.Email)).Returns((ErrorOr<User>)Error.NotFound());
        this.mockHashService.Setup(m => m.GetHash(It.IsAny<string>())).Returns((ErrorOr<string>)Error.Failure("hash_failed"));

        // Act
        var result = this.authService.OAuthRegister(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("hash_failed");
    }

    [Fact]
    public void OAuthRegister_WhenUserDoesNotExist_AndCreateUserFails_ReturnsError()
    {
        // Arrange
        var request = new OAuthRegisterRequest { Email = "test@test.com", Provider = "Google", ProviderToken = "token", FullName = "Name" };
        this.mockAuthRepository.Setup(m => m.FindOAuthLink(request.Provider, request.ProviderToken)).Returns((ErrorOr<OAuthLink>)Error.NotFound());
        this.mockAuthRepository.Setup(m => m.FindUserByEmail(request.Email)).Returns((ErrorOr<User>)Error.NotFound());
        this.mockHashService.Setup(m => m.GetHash(It.IsAny<string>())).Returns((ErrorOr<string>)"hash");
        this.mockAuthRepository.Setup(m => m.CreateUser(It.IsAny<User>())).Returns((ErrorOr<Success>)Error.Failure("create_user_failed"));

        // Act
        var result = this.authService.OAuthRegister(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("user_creation_failed");
    }

    [Fact]
    public void OAuthRegister_WhenUserDoesNotExist_AndUserRetrievalFails_ReturnsError()
    {
        // Arrange
        var request = new OAuthRegisterRequest { Email = "test@test.com", Provider = "Google", ProviderToken = "token", FullName = "Name" };
        this.mockAuthRepository.Setup(m => m.FindOAuthLink(request.Provider, request.ProviderToken)).Returns((ErrorOr<OAuthLink>)Error.NotFound());
        this.mockAuthRepository.SetupSequence(m => m.FindUserByEmail(request.Email))
            .Returns((ErrorOr<User>)Error.NotFound())
            .Returns((ErrorOr<User>)Error.Failure("retrieval_failed"));
        this.mockHashService.Setup(m => m.GetHash(It.IsAny<string>())).Returns((ErrorOr<string>)"hash");
        this.mockAuthRepository.Setup(m => m.CreateUser(It.IsAny<User>())).Returns(Result.Success);

        // Act
        var result = this.authService.OAuthRegister(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("user_retrieval_failed");
    }

    [Fact]
    public void OAuthRegister_WhenUserDoesNotExist_AndCreateLinkSucceeds_ReturnsSuccess()
    {
        // Arrange
        var request = new OAuthRegisterRequest { Email = "test@test.com", Provider = "Google", ProviderToken = "token", FullName = "Name" };
        var user = new User { Id = 1, Email = request.Email };
        this.mockAuthRepository.Setup(m => m.FindOAuthLink(request.Provider, request.ProviderToken)).Returns((ErrorOr<OAuthLink>)Error.NotFound());
        this.mockAuthRepository.SetupSequence(m => m.FindUserByEmail(request.Email))
            .Returns((ErrorOr<User>)Error.NotFound())
            .Returns((ErrorOr<User>)user);
        this.mockHashService.Setup(m => m.GetHash(It.IsAny<string>())).Returns((ErrorOr<string>)"hash");
        this.mockAuthRepository.Setup(m => m.CreateUser(It.IsAny<User>())).Returns(Result.Success);
        this.mockAuthRepository.Setup(m => m.CreateOAuthLink(It.IsAny<OAuthLink>())).Returns(Result.Success);

        // Act
        var result = this.authService.OAuthRegister(request);

        // Assert
        result.IsError.Should().BeFalse();
    }

    [Fact]
    public void VerifyOTP_WhenUserNotFound_ReturnsError()
    {
        // Arrange
        var request = new VerifyOTPRequest { UserId = 1, OTPCode = "123456" };
        this.mockAuthRepository.Setup(m => m.FindUserById(request.UserId)).Returns((ErrorOr<User>)Error.NotFound("user_not_found"));

        // Act
        var result = this.authService.VerifyOTP(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("user_not_found");
    }

    [Fact]
    public void VerifyOTP_WhenVerifyTOTPFails_ReturnsError()
    {
        // Arrange
        var request = new VerifyOTPRequest { UserId = 1, OTPCode = "123456" };
        var user = new User { Id = 1 };
        this.mockAuthRepository.Setup(m => m.FindUserById(request.UserId)).Returns((ErrorOr<User>)user);
        this.mockOtpService.Setup(m => m.VerifyTOTP(request.UserId, request.OTPCode)).Returns((ErrorOr<bool>)Error.Failure("totp_failed"));

        // Act
        var result = this.authService.VerifyOTP(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("totp_failed");
    }

    [Fact]
    public void VerifyOTP_WhenOtpInvalid_ReturnsUnauthorized()
    {
        // Arrange
        var request = new VerifyOTPRequest { UserId = 1, OTPCode = "000000" };
        var user = new User { Id = 1 };
        this.mockAuthRepository.Setup(m => m.FindUserById(request.UserId)).Returns((ErrorOr<User>)user);
        this.mockOtpService.Setup(m => m.VerifyTOTP(request.UserId, request.OTPCode)).Returns((ErrorOr<bool>)false);

        // Act
        var result = this.authService.VerifyOTP(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("invalid_otp");
    }

    [Fact]
    public void VerifyOTP_WhenValid_ReturnsFullLogin()
    {
        // Arrange
        var request = new VerifyOTPRequest { UserId = 1, OTPCode = "123456" };
        var user = new User { Id = 1, Email = "test@user.com" };
        this.mockAuthRepository.Setup(m => m.FindUserById(request.UserId)).Returns((ErrorOr<User>)user);
        this.mockOtpService.Setup(m => m.VerifyTOTP(request.UserId, request.OTPCode)).Returns((ErrorOr<bool>)true);
        this.mockAuthRepository.Setup(m => m.ResetFailedAttempts(user.Id)).Returns(Result.Success);
        this.mockJwtService.Setup(m => m.GenerateToken(user.Id)).Returns((ErrorOr<string>)"token");
        this.mockAuthRepository.Setup(m => m.CreateSession(user.Id, "token", null, null, null)).Returns((ErrorOr<Session>)new Session());

        // Act
        var result = this.authService.VerifyOTP(request);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeOfType<FullLogin>();
        this.mockOtpService.Verify(m => m.InvalidateOTP(user.Id), Times.Once);
    }

    [Fact]
    public void ResendOTP_WhenUserNotFound_ReturnsError()
    {
        // Arrange
        this.mockAuthRepository.Setup(m => m.FindUserById(1)).Returns((ErrorOr<User>)Error.NotFound("user_not_found"));

        // Act
        var result = this.authService.ResendOTP(1, "Email");

        // Assert
        result.IsError.Should().BeTrue();
    }

    [Fact]
    public void ResendOTP_WhenGenerateTOTPFails_ReturnsError()
    {
        // Arrange
        var user = new User { Id = 1 };
        this.mockAuthRepository.Setup(m => m.FindUserById(1)).Returns((ErrorOr<User>)user);
        this.mockOtpService.Setup(m => m.GenerateTOTP(1)).Returns((ErrorOr<string>)Error.Failure("totp_failed"));

        // Act
        var result = this.authService.ResendOTP(1, "Email");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("totp_failed");
    }

    [Fact]
    public void ResendOTP_WhenValid_SendsEmailAndReturnsSuccess()
    {
        // Arrange
        var user = new User { Id = 1, Email = "test@user.com", Preferred2FAMethod = "Email" };
        this.mockAuthRepository.Setup(m => m.FindUserById(user.Id)).Returns((ErrorOr<User>)user);
        this.mockOtpService.Setup(m => m.GenerateTOTP(user.Id)).Returns((ErrorOr<string>)"123456");

        // Act
        var result = this.authService.ResendOTP(user.Id, "Email");

        // Assert
        result.IsError.Should().BeFalse();
        this.mockEmailService.Verify(m => m.SendOTPCode(user.Email, "123456"), Times.Once);
    }

    [Fact]
    public void ResendOTP_WhenValidAndMethodIsNotEmail_ReturnsSuccessWithoutEmail()
    {
        // Arrange
        var user = new User { Id = 1, Email = "test@test.com", Preferred2FAMethod = "SMS" };
        this.mockAuthRepository.Setup(m => m.FindUserById(user.Id)).Returns((ErrorOr<User>)user);
        this.mockOtpService.Setup(m => m.GenerateTOTP(user.Id)).Returns((ErrorOr<string>)"123456");

        // Act
        var result = this.authService.ResendOTP(user.Id, "SMS");

        // Assert
        result.IsError.Should().BeFalse();
        this.mockEmailService.Verify(m => m.SendOTPCode(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void RequestPasswordReset_WhenUserNotFound_ReturnsError()
    {
        // Arrange
        this.mockAuthRepository.Setup(m => m.FindUserByEmail("test@test.com")).Returns((ErrorOr<User>)Error.NotFound("user_not_found"));

        // Act
        var result = this.authService.RequestPasswordReset("test@test.com");

        // Assert
        result.IsError.Should().BeTrue();
    }

    [Fact]
    public void RequestPasswordReset_WhenSaveTokenFails_ReturnsError()
    {
        // Arrange
        var user = new User { Id = 1, Email = "test@test.com" };
        this.mockAuthRepository.Setup(m => m.FindUserByEmail(user.Email)).Returns((ErrorOr<User>)user);
        this.mockAuthRepository.Setup(m => m.SavePasswordResetToken(It.IsAny<PasswordResetToken>())).Returns((ErrorOr<Success>)Error.Failure("save_failed"));

        // Act
        var result = this.authService.RequestPasswordReset(user.Email);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Description.Should().Be("Failed to save password reset token.");
    }

    [Fact]
    public void RequestPasswordReset_WhenUserFound_SendsEmailAndReturnsSuccess()
    {
        // Arrange
        var user = new User { Id = 1, Email = "test@test.com" };
        this.mockAuthRepository.Setup(m => m.FindUserByEmail(user.Email)).Returns((ErrorOr<User>)user);
        this.mockAuthRepository.Setup(m => m.DeleteExpiredPasswordResetTokens()).Returns(Result.Success);
        this.mockAuthRepository.Setup(m => m.SavePasswordResetToken(It.IsAny<PasswordResetToken>())).Returns(Result.Success);

        // Act
        var result = this.authService.RequestPasswordReset(user.Email);

        // Assert
        result.IsError.Should().BeFalse();
        this.mockEmailService.Verify(m => m.SendPasswordResetLink(It.Is<string>(e => e == user.Email), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void ResetPassword_WhenTokenIsNullOrWhiteSpace_ReturnsError()
    {
        // Arrange & Act
        var result = this.authService.ResetPassword(string.Empty, "ValidPassword123!");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("token_invalid");
    }

    [Fact]
    public void ResetPassword_WhenTokenNotFound_ReturnsError()
    {
        // Arrange
        this.mockAuthRepository.Setup(m => m.FindPasswordResetToken(It.IsAny<string>())).Returns((ErrorOr<PasswordResetToken>)Error.NotFound("token_not_found"));

        // Act
        var result = this.authService.ResetPassword("token", "ValidPassword123!");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("token_invalid");
    }

    [Fact]
    public void ResetPassword_WhenTokenUsed_ReturnsError()
    {
        // Arrange
        var token = new PasswordResetToken { UsedAt = DateTime.UtcNow };
        this.mockAuthRepository.Setup(m => m.FindPasswordResetToken(It.IsAny<string>())).Returns((ErrorOr<PasswordResetToken>)token);

        // Act
        var result = this.authService.ResetPassword("token", "ValidPassword123!");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("token_already_used");
    }

    [Fact]
    public void ResetPassword_WhenTokenExpired_ReturnsValidationError()
    {
        // Arrange
        var token = new PasswordResetToken { UserId = 1, ExpiresAt = DateTime.UtcNow.AddMinutes(-5) };
        this.mockAuthRepository.Setup(m => m.FindPasswordResetToken(It.IsAny<string>())).Returns((ErrorOr<PasswordResetToken>)token);

        // Act
        var result = this.authService.ResetPassword("raw_token", "NewValidPassword123!");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("token_expired");
    }

    [Fact]
    public void ResetPassword_WhenHashGenerationFails_ReturnsError()
    {
        // Arrange
        var token = new PasswordResetToken { ExpiresAt = DateTime.UtcNow.AddMinutes(5) };
        this.mockAuthRepository.Setup(m => m.FindPasswordResetToken(It.IsAny<string>())).Returns((ErrorOr<PasswordResetToken>)token);
        this.mockHashService.Setup(m => m.GetHash(It.IsAny<string>())).Returns((ErrorOr<string>)Error.Failure("hash_failed"));

        // Act
        var result = this.authService.ResetPassword("token", "ValidPassword123!");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("hash_failed");
    }

    [Fact]
    public void ResetPassword_WhenUpdatePasswordFails_ReturnsError()
    {
        // Arrange
        var token = new PasswordResetToken { UserId = 1, ExpiresAt = DateTime.UtcNow.AddMinutes(5) };
        this.mockAuthRepository.Setup(m => m.FindPasswordResetToken(It.IsAny<string>())).Returns((ErrorOr<PasswordResetToken>)token);
        this.mockHashService.Setup(m => m.GetHash(It.IsAny<string>())).Returns((ErrorOr<string>)"hash");
        this.mockAuthRepository.Setup(m => m.UpdatePassword(token.UserId, "hash")).Returns((ErrorOr<Success>)Error.Failure("update_failed"));

        // Act
        var result = this.authService.ResetPassword("token", "ValidPassword123!");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("token_invalid");
    }

    [Fact]
    public void ResetPassword_WhenMarkTokenAsUsedFails_ReturnsError()
    {
        // Arrange
        var token = new PasswordResetToken { Id = 1, UserId = 1, ExpiresAt = DateTime.UtcNow.AddMinutes(5) };
        this.mockAuthRepository.Setup(m => m.FindPasswordResetToken(It.IsAny<string>())).Returns((ErrorOr<PasswordResetToken>)token);
        this.mockHashService.Setup(m => m.GetHash(It.IsAny<string>())).Returns((ErrorOr<string>)"new_hash");
        this.mockAuthRepository.Setup(m => m.UpdatePassword(token.UserId, "new_hash")).Returns(Result.Success);
        this.mockAuthRepository.Setup(m => m.MarkPasswordResetTokenAsUsed(token.Id)).Returns((ErrorOr<Success>)Error.Failure("mark_failed"));

        // Act
        var result = this.authService.ResetPassword("raw_token", "ValidPassword123!");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("reset_failed");
    }

    [Fact]
    public void ResetPassword_WhenInvalidateSessionsFails_ReturnsError()
    {
        // Arrange
        var token = new PasswordResetToken { Id = 1, UserId = 1, ExpiresAt = DateTime.UtcNow.AddMinutes(5) };
        this.mockAuthRepository.Setup(m => m.FindPasswordResetToken(It.IsAny<string>())).Returns((ErrorOr<PasswordResetToken>)token);
        this.mockHashService.Setup(m => m.GetHash(It.IsAny<string>())).Returns((ErrorOr<string>)"new_hash");
        this.mockAuthRepository.Setup(m => m.UpdatePassword(token.UserId, "new_hash")).Returns(Result.Success);
        this.mockAuthRepository.Setup(m => m.MarkPasswordResetTokenAsUsed(token.Id)).Returns(Result.Success);
        this.mockAuthRepository.Setup(m => m.InvalidateAllSessions(token.UserId)).Returns((ErrorOr<Success>)Error.Failure("invalidate_failed"));

        // Act
        var result = this.authService.ResetPassword("raw_token", "ValidPassword123!");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("reset_failed");
    }

    [Fact]
    public void ResetPassword_WhenValid_UpdatesPasswordAndReturnsSuccess()
    {
        // Arrange
        var token = new PasswordResetToken { Id = 1, UserId = 1, ExpiresAt = DateTime.UtcNow.AddMinutes(5), UsedAt = null };
        var newPassword = "NewValidPassword123!";
        this.mockAuthRepository.Setup(m => m.FindPasswordResetToken(It.IsAny<string>())).Returns((ErrorOr<PasswordResetToken>)token);
        this.mockHashService.Setup(m => m.GetHash(newPassword)).Returns((ErrorOr<string>)"new_hash");
        this.mockAuthRepository.Setup(m => m.UpdatePassword(token.UserId, "new_hash")).Returns(Result.Success);
        this.mockAuthRepository.Setup(m => m.MarkPasswordResetTokenAsUsed(token.Id)).Returns(Result.Success);
        this.mockAuthRepository.Setup(m => m.InvalidateAllSessions(token.UserId)).Returns(Result.Success);

        // Act
        var result = this.authService.ResetPassword("raw_token", newPassword);

        // Assert
        result.IsError.Should().BeFalse();
        this.mockAuthRepository.Verify(m => m.UpdatePassword(token.UserId, "new_hash"), Times.Once);
        this.mockAuthRepository.Verify(m => m.InvalidateAllSessions(token.UserId), Times.Once);
    }

    [Fact]
    public void VerifyResetToken_WhenTokenIsNullOrWhiteSpace_ReturnsError()
    {
        // Arrange & Act
        var result = this.authService.VerifyResetToken(string.Empty);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("token_invalid");
    }

    [Fact]
    public void VerifyResetToken_WhenTokenNotFound_ReturnsError()
    {
        // Arrange
        this.mockAuthRepository.Setup(m => m.FindPasswordResetToken(It.IsAny<string>())).Returns((ErrorOr<PasswordResetToken>)Error.NotFound("not_found"));

        // Act
        var result = this.authService.VerifyResetToken("token");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("token_invalid");
    }

    [Fact]
    public void VerifyResetToken_WhenTokenValid_ReturnsSuccess()
    {
        // Arrange
        var token = new PasswordResetToken { ExpiresAt = DateTime.UtcNow.AddMinutes(5) };
        this.mockAuthRepository.Setup(m => m.FindPasswordResetToken(It.IsAny<string>())).Returns((ErrorOr<PasswordResetToken>)token);

        // Act
        var result = this.authService.VerifyResetToken("token");

        // Assert
        result.IsError.Should().BeFalse();
    }

    [Fact]
    public void Logout_WhenSessionNotFound_ReturnsError()
    {
        // Arrange
        this.mockAuthRepository.Setup(m => m.FindSessionByToken("invalid")).Returns((ErrorOr<Session>)Error.NotFound());

        // Act
        var result = this.authService.Logout("invalid");

        // Assert
        result.IsError.Should().BeTrue();
    }

    [Fact]
    public void Logout_WhenValid_ReturnsSuccess()
    {
        // Arrange
        var session = new Session { Id = 1, UserId = 1 };
        this.mockAuthRepository.Setup(m => m.FindSessionByToken("valid_token")).Returns((ErrorOr<Session>)session);
        this.mockAuthRepository.Setup(m => m.UpdateSessionToken(session.Id)).Returns(Result.Success);

        // Act
        var result = this.authService.Logout("valid_token");

        // Assert
        result.IsError.Should().BeFalse();
        this.mockAuthRepository.Verify(m => m.UpdateSessionToken(session.Id), Times.Once);
    }
}