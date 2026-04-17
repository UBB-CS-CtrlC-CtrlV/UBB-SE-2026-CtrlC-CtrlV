// <copyright file="LoginServiceTests.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankApp.Application.DTOs.Auth;
using BankApp.Application.Repositories.Interfaces;
using BankApp.Application.Services.Login;
using BankApp.Application.Services.Notifications;
using BankApp.Application.Services.Security;
using BankApp.Domain.Entities;
using ErrorOr;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using MockFactory = BankApp.Application.Tests.MockFactory;

namespace BankApp.Application.Tests.Services;

/// <summary>
/// Unit tests for <see cref="LoginService"/>.
/// </summary>
public class LoginServiceTests
{
    private readonly Mock<IAuthRepository> authRepository = MockFactory.CreateAuthRepository();
    private readonly Mock<IHashService> hashService = MockFactory.CreateHashService();
    private readonly Mock<IJwtService> jwtService = MockFactory.CreateJwtService();
    private readonly Mock<IOtpService> otpService = MockFactory.CreateOtpService();
    private readonly Mock<IEmailService> emailService = MockFactory.CreateEmailService();
    private readonly LoginService service;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginServiceTests"/> class.
    /// </summary>
    public LoginServiceTests()
    {
        this.service = new LoginService(
            this.authRepository.Object,
            this.hashService.Object,
            this.jwtService.Object,
            this.otpService.Object,
            this.emailService.Object,
            NullLogger<LoginService>.Instance);
    }

    /// <summary>
    /// TODO: add docs.
    /// </summary>
    [Fact]
    public void Login_WhenMaxFailedAttemptsReached_LocksForFifteenMinutes()
    {
        // Arrange
        var request = new LoginRequest { Email = "ada@test.com", Password = "wrong" };
        var user = new User
        {
            Id = 1,
            Email = request.Email,
            PasswordHash = "hash",
            FailedLoginAttempts = 4,
        };
        DateTime before = DateTime.UtcNow.AddMinutes(14);

        this.authRepository.Setup(repository => repository.FindUserByEmail(request.Email))
            .Returns((ErrorOr<User>)user);
        this.hashService.Setup(service => service.Verify(request.Password, user.PasswordHash))
            .Returns((ErrorOr<bool>)false);

        // Act
        ErrorOr<LoginSuccess> result = this.service.Login(request);

        // Assert
        result.IsError.Should().BeTrue();
        this.authRepository.Verify(
            repository => repository.LockAccount(
                user.Id,
                It.Is<DateTime>(lockoutEnd =>
                    lockoutEnd >= before && lockoutEnd <= DateTime.UtcNow.AddMinutes(16))),
            Times.Once);
    }

    /// <summary>
    /// TODO: add docs.
    /// </summary>
    [Fact]
    public void Login_WhenEmailTwoFactorIsEnabled_GeneratesEmailOtp()
    {
        // Arrange
        var request = new LoginRequest { Email = "ada@test.com", Password = "ValidPass1!" };
        var user = new User
        {
            Id = 1,
            Email = request.Email,
            PasswordHash = "hash",
            Is2FAEnabled = true,
            Preferred2FAMethod = "Email",
        };

        this.authRepository.Setup(repository => repository.FindUserByEmail(request.Email))
            .Returns((ErrorOr<User>)user);
        this.hashService.Setup(service => service.Verify(request.Password, user.PasswordHash))
            .Returns((ErrorOr<bool>)true);
        this.otpService.Setup(service => service.GenerateSMSOTP(user.Id))
            .Returns((ErrorOr<string>)"123456");

        // Act
        ErrorOr<LoginSuccess> result = this.service.Login(request);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeOfType<RequiresTwoFactor>();
        this.emailService.Verify(service => service.SendOTPCode(user.Email, "123456"), Times.Once);
    }

    /// <summary>
    /// TODO: add docs.
    /// </summary>
    [Fact]
    public void VerifyOtp_WhenThreeInvalidAttempts_InvalidatesOtpAndRequiresRestart()
    {
        // Arrange
        var request = new VerifyOTPRequest { UserId = 1, OTPCode = "000000" };
        var user = new User { Id = request.UserId, Preferred2FAMethod = "Email" };

        this.authRepository.Setup(repository => repository.FindUserById(user.Id))
            .Returns((ErrorOr<User>)user);
        this.otpService.Setup(service => service.VerifySMSOTP(user.Id, request.OTPCode))
            .Returns((ErrorOr<bool>)false);

        // Act
        _ = this.service.VerifyOTP(request);
        _ = this.service.VerifyOTP(request);
        ErrorOr<LoginSuccess> result = this.service.VerifyOTP(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("otp_attempts_exceeded");
        this.otpService.Verify(service => service.InvalidateOTP(user.Id), Times.Once);
    }
}