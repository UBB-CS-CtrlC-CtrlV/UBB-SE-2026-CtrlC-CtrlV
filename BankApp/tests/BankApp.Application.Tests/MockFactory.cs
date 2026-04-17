// <copyright file="MockFactory.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System.Security.Claims;
using BankApp.Application.Repositories.Interfaces;
using BankApp.Application.Services.Notifications;
using BankApp.Application.Services.Security;
using BankApp.Domain.Entities;
using ErrorOr;
using Moq;

namespace BankApp.Application.Tests;

/// <summary>
/// Factory methods for creating Moq mocks with sensible default return values.
/// </summary>
internal static class MockFactory
{
    /// <summary>
    /// TODO: add docs.
    /// </summary>
    /// <returns>TODO: returns something.</returns>
    internal static Mock<IAuthRepository> CreateAuthRepository()
    {
        var mock = new Mock<IAuthRepository>(MockBehavior.Strict);

        mock.Setup(repository => repository.FindUserByEmail(It.IsAny<string>()))
            .Returns((ErrorOr<User>)Error.NotFound());
        mock.Setup(repository => repository.CreateUser(It.IsAny<User>()))
            .Returns(Result.Success);
        mock.Setup(repository => repository.FindOAuthLink(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((ErrorOr<OAuthLink>)Error.NotFound());
        mock.Setup(repository => repository.CreateOAuthLink(It.IsAny<OAuthLink>()))
            .Returns(Result.Success);
        mock.Setup(repository => repository.CreateSession(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>()))
            .Returns(new Session());
        mock.Setup(repository => repository.FindSessionByToken(It.IsAny<string>()))
            .Returns((ErrorOr<Session>)Error.NotFound());
        mock.Setup(repository => repository.SavePasswordResetToken(It.IsAny<PasswordResetToken>()))
            .Returns(Result.Success);
        mock.Setup(repository => repository.FindPasswordResetToken(It.IsAny<string>()))
            .Returns((ErrorOr<PasswordResetToken>)Error.NotFound());
        mock.Setup(repository => repository.MarkPasswordResetTokenAsUsed(It.IsAny<int>()))
            .Returns(Result.Success);
        mock.Setup(repository => repository.DeleteExpiredPasswordResetTokens())
            .Returns(Result.Success);
        mock.Setup(repository => repository.InvalidateAllSessions(It.IsAny<int>()))
            .Returns(Result.Success);
        mock.Setup(repository => repository.FindUserById(It.IsAny<int>()))
            .Returns((ErrorOr<User>)Error.NotFound());
        mock.Setup(repository => repository.UpdatePassword(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(Result.Success);
        mock.Setup(repository => repository.FindSessionsByUserId(It.IsAny<int>()))
            .Returns(new List<Session>());
        mock.Setup(repository => repository.UpdateSessionToken(It.IsAny<int>()))
            .Returns(Result.Success);
        mock.Setup(repository => repository.IncrementFailedAttempts(It.IsAny<int>()))
            .Returns(Result.Success);
        mock.Setup(repository => repository.ResetFailedAttempts(It.IsAny<int>()))
            .Returns(Result.Success);
        mock.Setup(repository => repository.LockAccount(It.IsAny<int>(), It.IsAny<DateTime>()))
            .Returns(Result.Success);

        return mock;
    }

    /// <summary>
    /// TODO: add docs.
    /// </summary>
    /// <returns>TODO: returns something.</returns>
    internal static Mock<IHashService> CreateHashService()
    {
        var mock = new Mock<IHashService>(MockBehavior.Strict);

        mock.Setup(service => service.GetHash(It.IsAny<string>()))
            .Returns((string input) => $"hashed_{input}");
        mock.Setup(service => service.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);

        return mock;
    }

    /// <summary>
    /// TODO: add docs.
    /// </summary>
    /// <returns>TODO: returns something.</returns>
    internal static Mock<IJwtService> CreateJwtService()
    {
        var mock = new Mock<IJwtService>(MockBehavior.Strict);

        mock.Setup(service => service.GenerateToken(It.IsAny<int>()))
            .Returns("jwt-token");
        mock.Setup(service => service.ValidateToken(It.IsAny<string>()))
            .Returns(new ClaimsPrincipal());
        mock.Setup(service => service.ExtractUserId(It.IsAny<string>()))
            .Returns(0);

        return mock;
    }

    /// <summary>
    /// TODO: add docs.
    /// </summary>
    /// <returns>TODO: returns something.</returns>
    internal static Mock<IOtpService> CreateOtpService()
    {
        var mock = new Mock<IOtpService>(MockBehavior.Strict);

        mock.Setup(service => service.GenerateTOTP(It.IsAny<int>()))
            .Returns("123456");
        mock.Setup(service => service.VerifyTOTP(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(true);
        mock.Setup(service => service.GenerateSMSOTP(It.IsAny<int>()))
            .Returns("123456");
        mock.Setup(service => service.VerifySMSOTP(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(true);
        mock.Setup(service => service.IsExpired(It.IsAny<DateTime>()))
            .Returns((DateTime expiresAt) => DateTime.UtcNow > expiresAt);
        mock.Setup(service => service.InvalidateOTP(It.IsAny<int>()));

        return mock;
    }

    /// <summary>
    /// TODO: add docs.
    /// </summary>
    /// <returns>TODO: returns something.</returns>
    internal static Mock<IEmailService> CreateEmailService()
    {
        var mock = new Mock<IEmailService>(MockBehavior.Strict);

        mock.Setup(service => service.SendOTPCode(It.IsAny<string>(), It.IsAny<string>()));
        mock.Setup(service => service.SendPasswordResetLink(It.IsAny<string>(), It.IsAny<string>()));
        mock.Setup(service => service.SendLockNotification(It.IsAny<string>()));
        mock.Setup(service => service.SendLoginAlert(It.IsAny<string>()));

        return mock;
    }
}
