// Copyright (c) BankApp. All rights reserved.
// Licensed under the MIT license.

using BankApp.Contracts.DTOs.Auth;
using BankApp.Contracts.DTOs.Dashboard;
using BankApp.Contracts.DTOs.Profile;
using BankApp.Contracts.Entities;
using BankApp.Server.Repositories.Interfaces;
using BankApp.Server.Services.Dashboard;
using BankApp.Server.Services.Login;
using BankApp.Server.Services.PasswordRecovery;
using BankApp.Server.Services.Profile;
using BankApp.Server.Services.Registration;
using BankApp.Server.Services.Security;
using ErrorOr;
using NSubstitute;
using NSubstitute.Extensions;

namespace BankApp.Server.Tests;

/// <summary>
/// Factory methods for creating NSubstitute substitutes whose interfaces return
/// <c>ErrorOr&lt;T&gt;</c>.  Because <c>ErrorOr&lt;T&gt;</c> is a struct with a
/// throwing parameterless constructor, we must register sensible default return
/// values via <see cref="ReturnsForAllExtensions.ReturnsForAll{T}"/> before any
/// specific <c>.Returns()</c> setup is applied.
/// </summary>
internal static class SubstituteFactory
{
    public static ILoginService CreateLoginService()
    {
        var sub = Substitute.For<ILoginService>();
        sub.ReturnsForAll<ErrorOr<LoginSuccess>>(new FullLogin(0, string.Empty));
        sub.ReturnsForAll<ErrorOr<Success>>(Result.Success);
        return sub;
    }

    public static IRegistrationService CreateRegistrationService()
    {
        var sub = Substitute.For<IRegistrationService>();
        sub.ReturnsForAll<ErrorOr<Success>>(Result.Success);
        return sub;
    }

    public static IPasswordRecoveryService CreatePasswordRecoveryService()
    {
        var sub = Substitute.For<IPasswordRecoveryService>();
        sub.ReturnsForAll<ErrorOr<Success>>(Result.Success);
        return sub;
    }

    public static IDashboardService CreateDashboardService()
    {
        var sub = Substitute.For<IDashboardService>();
        sub.ReturnsForAll<ErrorOr<DashboardResponse>>(new DashboardResponse());
        return sub;
    }

    public static IProfileService CreateProfileService()
    {
        var sub = Substitute.For<IProfileService>();
        sub.ReturnsForAll<ErrorOr<ProfileInfo>>(new ProfileInfo());
        sub.ReturnsForAll<ErrorOr<Success>>(Result.Success);
        sub.ReturnsForAll<ErrorOr<List<OAuthLinkDto>>>(new List<OAuthLinkDto>());
        sub.ReturnsForAll<ErrorOr<List<NotificationPreferenceDto>>>(new List<NotificationPreferenceDto>());
        sub.ReturnsForAll<ErrorOr<bool>>(true);
        sub.ReturnsForAll<ErrorOr<List<SessionDto>>>(new List<SessionDto>());
        return sub;
    }

    public static IJwtService CreateJwtService()
    {
        var sub = Substitute.For<IJwtService>();
        sub.ReturnsForAll<ErrorOr<int>>(0);
        return sub;
    }

    public static IAuthRepository CreateAuthRepository()
    {
        var sub = Substitute.For<IAuthRepository>();
        sub.ReturnsForAll<ErrorOr<Session>>(new Session());
        sub.ReturnsForAll<ErrorOr<Success>>(Result.Success);
        return sub;
    }
}
