// <copyright file="PasswordRecoveryServiceTests.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankApp.Application.Repositories.Interfaces;
using BankApp.Application.Services.Notifications;
using BankApp.Application.Services.PasswordRecovery;
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
/// Unit tests for <see cref="PasswordRecoveryService"/>.
/// </summary>
public class PasswordRecoveryServiceTests
{
    /// <summary>
    /// TODO: add docs.
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
}
