// <copyright file="MockFactory.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankApp.Application.Repositories.Interfaces;
using BankApp.Application.Services.Dashboard;
using BankApp.Application.Services.Login;
using BankApp.Application.Services.PasswordRecovery;
using BankApp.Application.Services.Profile;
using BankApp.Application.Services.Registration;
using BankApp.Application.Services.Security;
using Moq;

namespace BankApp.Api.Tests.Integration.Infrastructure;

/// <summary>
/// Creates pre-configured Moq stubs for the service and repository interfaces
/// used across integration tests. All mocks use loose behaviour so individual
/// tests can override specific setups without affecting unrelated calls.
/// </summary>
public static class MockFactory
{
    /// <summary>Creates a loose mock for <see cref="IJwtService"/>.</summary>
    /// <returns>A new <see cref="Mock{T}"/> of <see cref="IJwtService"/>.</returns>
    public static Mock<IJwtService> CreateJwtService() => new Mock<IJwtService>();

    /// <summary>Creates a loose mock for <see cref="IAuthRepository"/>.</summary>
    /// <returns>A new <see cref="Mock{T}"/> of <see cref="IAuthRepository"/>.</returns>
    public static Mock<IAuthRepository> CreateAuthRepository() => new Mock<IAuthRepository>();

    /// <summary>Creates a loose mock for <see cref="ILoginService"/>.</summary>
    /// <returns>A new <see cref="Mock{T}"/> of <see cref="ILoginService"/>.</returns>
    public static Mock<ILoginService> CreateLoginService() => new Mock<ILoginService>();

    /// <summary>Creates a loose mock for <see cref="IRegistrationService"/>.</summary>
    /// <returns>A new <see cref="Mock{T}"/> of <see cref="IRegistrationService"/>.</returns>
    public static Mock<IRegistrationService> CreateRegistrationService() => new Mock<IRegistrationService>();

    /// <summary>Creates a loose mock for <see cref="IPasswordRecoveryService"/>.</summary>
    /// <returns>A new <see cref="Mock{T}"/> of <see cref="IPasswordRecoveryService"/>.</returns>
    public static Mock<IPasswordRecoveryService> CreatePasswordRecoveryService() => new Mock<IPasswordRecoveryService>();

    /// <summary>Creates a loose mock for <see cref="IDashboardService"/>.</summary>
    /// <returns>A new <see cref="Mock{T}"/> of <see cref="IDashboardService"/>.</returns>
    public static Mock<IDashboardService> CreateDashboardService() => new Mock<IDashboardService>();

    /// <summary>Creates a loose mock for <see cref="IProfileService"/>.</summary>
    /// <returns>A new <see cref="Mock{T}"/> of <see cref="IProfileService"/>.</returns>
    public static Mock<IProfileService> CreateProfileService() => new Mock<IProfileService>();
}
