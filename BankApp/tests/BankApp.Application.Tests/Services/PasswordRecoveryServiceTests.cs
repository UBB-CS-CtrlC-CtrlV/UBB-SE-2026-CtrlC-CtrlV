// <copyright file="PasswordRecoveryServiceTests.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankApp.Application.Repositories.Interfaces;
using BankApp.Application.Services.Notifications;
using BankApp.Application.Services.PasswordRecovery;
using BankApp.Application.Services.Security;
using BankApp.Domain.Entities;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;

namespace BankApp.Application.Tests.Services;

/// <summary>
/// Unit tests for <see cref="PasswordRecoveryService"/>.
/// </summary>
public class PasswordRecoveryServiceTests
{
    /// <summary>
    /// Verifies the RequestPasswordReset_WhenUserDoesNotExist_ReturnsRepositoryError scenario.
    /// </summary>
    [Fact]
    public void RequestPasswordReset_WhenUserDoesNotExist_ReturnsRepositoryError()
    {
        // Arrange
        Mock<IAuthRepository> authRepository = MockFactory.CreateAuthRepository();
        Mock<IHashService> hashService = MockFactory.CreateHashService();
        Mock<IEmailService> emailService = MockFactory.CreateEmailService();
        authRepository.Setup(repository => repository.FindUserByEmail("missing@test.com"))
            .Returns((ErrorOr<User>)Error.NotFound("user_not_found", "User not found."));

        var service = new PasswordRecoveryService(
            authRepository.Object,
            hashService.Object,
            emailService.Object,
            NullLogger<PasswordRecoveryService>.Instance);

        // Act
        ErrorOr<Success> result = service.RequestPasswordReset("missing@test.com");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("user_not_found");
        emailService.Verify(serviceMock => serviceMock.SendPasswordResetLink(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    /// <summary>
    /// Verifies the RequestPasswordReset_WhenUserExists_CreatesThirtyMinuteToken scenario.
    /// </summary>
    [Fact]
    public void RequestPasswordReset_WhenUserExists_CreatesThirtyMinuteToken()
    {
        // Arrange
        Mock<IAuthRepository> authRepository = MockFactory.CreateAuthRepository();
        Mock<IHashService> hashService = MockFactory.CreateHashService();
        Mock<IEmailService> emailService = MockFactory.CreateEmailService();
        var user = new User { Id = 1, Email = "ada@test.com" };
        PasswordResetToken? savedToken = null;
        DateTime before = DateTime.UtcNow.AddMinutes(29);

        authRepository.Setup(repository => repository.FindUserByEmail(user.Email))
            .Returns((ErrorOr<User>)user);
        authRepository.Setup(repository => repository.SavePasswordResetToken(It.IsAny<PasswordResetToken>()))
            .Callback<PasswordResetToken>(token => savedToken = token)
            .Returns(Result.Success);

        var service = new PasswordRecoveryService(
            authRepository.Object,
            hashService.Object,
            emailService.Object,
            NullLogger<PasswordRecoveryService>.Instance);

        // Act
        ErrorOr<Success> result = service.RequestPasswordReset(user.Email);

        // Assert
        result.IsError.Should().BeFalse();
        savedToken.Should().NotBeNull();
        savedToken!.ExpiresAt.Should().BeOnOrAfter(before);
        savedToken.ExpiresAt.Should().BeOnOrBefore(DateTime.UtcNow.AddMinutes(31));
    }

    /// <summary>
    /// Verifies the RequestPasswordReset_WhenSavingTokenFails_ReturnsFailure scenario.
    /// </summary>
    [Fact]
    public void RequestPasswordReset_WhenSavingTokenFails_ReturnsFailure()
    {
        // Arrange
        Mock<IAuthRepository> authRepository = MockFactory.CreateAuthRepository();
        Mock<IHashService> hashService = MockFactory.CreateHashService();
        Mock<IEmailService> emailService = MockFactory.CreateEmailService();
        var user = new User { Id = 1, Email = "ada@test.com" };

        authRepository.Setup(repository => repository.FindUserByEmail(user.Email))
            .Returns((ErrorOr<User>)user);
        authRepository.Setup(repository => repository.SavePasswordResetToken(It.IsAny<PasswordResetToken>()))
            .Returns(Error.Failure("save_failed", "Save failed."));

        var service = new PasswordRecoveryService(
            authRepository.Object,
            hashService.Object,
            emailService.Object,
            NullLogger<PasswordRecoveryService>.Instance);

        // Act
        ErrorOr<Success> result = service.RequestPasswordReset(user.Email);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Description.Should().Be("Failed to save password reset token.");
        emailService.Verify(serviceMock => serviceMock.SendPasswordResetLink(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    /// <summary>
    /// Verifies the ResetPassword_WhenTokenIsAlreadyUsed_ReturnsValidationError scenario.
    /// </summary>
    [Fact]
    public void ResetPassword_WhenTokenIsAlreadyUsed_ReturnsValidationError()
    {
        // Arrange
        Mock<IAuthRepository> authRepository = MockFactory.CreateAuthRepository();
        Mock<IHashService> hashService = MockFactory.CreateHashService();
        Mock<IEmailService> emailService = MockFactory.CreateEmailService();
        authRepository.Setup(repository => repository.FindPasswordResetToken(It.IsAny<string>()))
            .Returns(new PasswordResetToken
            {
                Id = 1,
                UserId = 1,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                UsedAt = DateTime.UtcNow,
            });

        var service = new PasswordRecoveryService(
            authRepository.Object,
            hashService.Object,
            emailService.Object,
            NullLogger<PasswordRecoveryService>.Instance);

        // Act
        ErrorOr<Success> result = service.ResetPassword("raw-token", "StrongPass1!");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("token_already_used");
    }

    /// <summary>
    /// Verifies the VerifyResetToken_WhenTokenExpired_ReturnsValidationError scenario.
    /// </summary>
    [Fact]
    public void VerifyResetToken_WhenTokenExpired_ReturnsValidationError()
    {
        // Arrange
        Mock<IAuthRepository> authRepository = MockFactory.CreateAuthRepository();
        Mock<IHashService> hashService = MockFactory.CreateHashService();
        Mock<IEmailService> emailService = MockFactory.CreateEmailService();
        authRepository.Setup(repository => repository.FindPasswordResetToken(It.IsAny<string>()))
            .Returns(new PasswordResetToken
            {
                Id = 1,
                UserId = 1,
                ExpiresAt = DateTime.UtcNow.AddMinutes(-1),
            });

        var service = new PasswordRecoveryService(
            authRepository.Object,
            hashService.Object,
            emailService.Object,
            NullLogger<PasswordRecoveryService>.Instance);

        // Act
        ErrorOr<Success> result = service.VerifyResetToken("raw-token");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("token_expired");
    }
}
