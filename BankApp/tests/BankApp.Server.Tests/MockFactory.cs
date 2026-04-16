// Copyright (c) BankApp. All rights reserved.
// Licensed under the MIT license.

using BankApp.Contracts.DTOs.Auth;
using BankApp.Contracts.DTOs.Dashboard;
using BankApp.Contracts.DTOs.Profile;
using BankApp.Contracts.Entities;
using BankApp.Server.Repositories.Interfaces;
using BankApp.Server.Services.Dashboard;
using BankApp.Server.Services.Login;
using BankApp.Server.Services.Notifications;
using BankApp.Server.Services.PasswordRecovery;
using BankApp.Server.Services.Profile;
using BankApp.Server.Services.Registration;
using BankApp.Server.Services.Security;
using ErrorOr;
using Moq;

namespace BankApp.Server.Tests;

/// <summary>
/// Factory methods for creating Moq mocks with sensible default return values.
/// </summary>
internal static class MockFactory
{
    public static Mock<ILoginService> CreateLoginService()
    {
        var mock = new Mock<ILoginService>(MockBehavior.Strict);
        mock.Setup(service => service.Login(It.IsAny<LoginRequest>(), It.IsAny<SessionMetadata?>()))
            .Returns((ErrorOr<LoginSuccess>)new FullLogin(0, string.Empty));
        mock.Setup(service => service.VerifyOTP(It.IsAny<VerifyOTPRequest>(), It.IsAny<SessionMetadata?>()))
            .Returns((ErrorOr<LoginSuccess>)new FullLogin(0, string.Empty));
        mock.Setup(service => service.OAuthLoginAsync(It.IsAny<OAuthLoginRequest>(), It.IsAny<SessionMetadata?>()))
            .ReturnsAsync((ErrorOr<LoginSuccess>)new FullLogin(0, string.Empty));
        mock.Setup(service => service.Logout(It.IsAny<string>()))
            .Returns(Result.Success);
        mock.Setup(service => service.ResendOTP(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(Result.Success);
        return mock;
    }

    public static Mock<IRegistrationService> CreateRegistrationService()
    {
        var mock = new Mock<IRegistrationService>(MockBehavior.Strict);
        mock.Setup(service => service.Register(It.IsAny<RegisterRequest>()))
            .Returns(Result.Success);
        mock.Setup(service => service.OAuthRegister(It.IsAny<OAuthRegisterRequest>()))
            .Returns(Result.Success);
        return mock;
    }

    public static Mock<IPasswordRecoveryService> CreatePasswordRecoveryService()
    {
        var mock = new Mock<IPasswordRecoveryService>(MockBehavior.Strict);
        mock.Setup(service => service.RequestPasswordReset(It.IsAny<string>()))
            .Returns(Result.Success);
        mock.Setup(service => service.ResetPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Result.Success);
        mock.Setup(service => service.VerifyResetToken(It.IsAny<string>()))
            .Returns(Result.Success);
        return mock;
    }

    public static Mock<IDashboardService> CreateDashboardService()
    {
        var mock = new Mock<IDashboardService>(MockBehavior.Strict);
        mock.Setup(service => service.GetDashboardData(It.IsAny<int>()))
            .Returns(new DashboardResponse());
        return mock;
    }

    public static Mock<IProfileService> CreateProfileService()
    {
        var mock = new Mock<IProfileService>(MockBehavior.Strict);
        mock.Setup(service => service.GetProfile(It.IsAny<int>()))
            .Returns(new ProfileInfo());
        mock.Setup(service => service.UpdatePersonalInfo(It.IsAny<UpdateProfileRequest>()))
            .Returns(Result.Success);
        mock.Setup(service => service.ChangePassword(It.IsAny<ChangePasswordRequest>()))
            .Returns(Result.Success);
        mock.Setup(service => service.Enable2FA(It.IsAny<int>(), It.IsAny<BankApp.Contracts.Enums.TwoFactorMethod>()))
            .Returns(Result.Success);
        mock.Setup(service => service.Disable2FA(It.IsAny<int>()))
            .Returns(Result.Success);
        mock.Setup(service => service.GetOAuthLinks(It.IsAny<int>()))
            .Returns(new List<OAuthLinkDto>());
        mock.Setup(service => service.GetNotificationPreferences(It.IsAny<int>()))
            .Returns(new List<NotificationPreferenceDto>());
        mock.Setup(service => service.UpdateNotificationPreferences(It.IsAny<int>(), It.IsAny<List<NotificationPreferenceDto>>()))
            .Returns(Result.Success);
        mock.Setup(service => service.VerifyPassword(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(true);
        mock.Setup(service => service.GetActiveSessions(It.IsAny<int>()))
            .Returns(new List<SessionDto>());
        mock.Setup(service => service.RevokeSession(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(Result.Success);
        return mock;
    }

    public static Mock<IJwtService> CreateJwtService()
    {
        var mock = new Mock<IJwtService>(MockBehavior.Strict);
        mock.Setup(service => service.ExtractUserId(It.IsAny<string>()))
            .Returns(0);
        return mock;
    }

    public static Mock<IAuthRepository> CreateAuthRepository()
    {
        var mock = new Mock<IAuthRepository>(MockBehavior.Strict);
        mock.Setup(repository => repository.FindSessionByToken(It.IsAny<string>()))
            .Returns(new Session());
        return mock;
    }

    public static Mock<IHashService> CreateHashService()
    {
        var mock = new Mock<IHashService>(MockBehavior.Strict);

        mock.Setup(service => service.GetHash(It.IsAny<string>()))
            .Returns((string input) => ErrorOrFactory.From($"hashed_{input}"));

        mock.Setup(service => service.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);

        return mock;
    }
    public static Mock<IOtpService> CreateOtpService()
    {
        var mock = new Mock<IOtpService>(MockBehavior.Strict);

        mock.Setup(service => service.GenerateTOTP(It.IsAny<int>()))
            .Returns("123456");

        mock.Setup(service => service.VerifyTOTP(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(true);

        mock.Setup(service => service.InvalidateOTP(It.IsAny<int>()));

        return mock;
    }
    public static Mock<IEmailService> CreateEmailService()
    {
        var mock = new Mock<IEmailService>(MockBehavior.Strict);

        mock.Setup(service => service.SendOTPCode(It.IsAny<string>(), It.IsAny<string>()));
        mock.Setup(service => service.SendPasswordResetLink(It.IsAny<string>(), It.IsAny<string>()));
        mock.Setup(service => service.SendLockNotification(It.IsAny<string>()));
        mock.Setup(service => service.SendLoginAlert(It.IsAny<string>()));

        return mock;
    }
}
