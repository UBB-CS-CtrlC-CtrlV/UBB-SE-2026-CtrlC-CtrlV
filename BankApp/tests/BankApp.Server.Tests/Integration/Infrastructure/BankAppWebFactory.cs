// <copyright file="BankAppWebFactory.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankApp.Contracts.Entities;
using BankApp.Server.Repositories.Interfaces;
using BankApp.Server.Services.Dashboard;
using BankApp.Server.Services.Login;
using BankApp.Server.Services.PasswordRecovery;
using BankApp.Server.Services.Profile;
using BankApp.Server.Services.Registration;
using BankApp.Server.Services.Security;
using ErrorOr;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace BankApp.Server.Tests.Integration.Infrastructure;

/// <summary>
/// A custom <see cref="WebApplicationFactory{TEntryPoint}"/> that replaces all
/// infrastructure services with Moq stubs so integration tests can exercise the
/// full HTTP pipeline without a database or external dependencies.
/// </summary>
public class BankAppWebFactory : WebApplicationFactory<Program>
{
    /// <summary>
    /// Gets the mock JWT service that controls token validation behaviour.
    /// </summary>
    public Mock<IJwtService> JwtServiceMock { get; } = new Mock<IJwtService>();

    /// <summary>
    /// Gets the mock auth repository that controls session lookup behaviour.
    /// </summary>
    public Mock<IAuthRepository> AuthRepositoryMock { get; } = new Mock<IAuthRepository>();

    /// <summary>
    /// Gets the mock login service.
    /// </summary>
    public Mock<ILoginService> LoginServiceMock { get; } = new Mock<ILoginService>();

    /// <summary>
    /// Gets the mock registration service.
    /// </summary>
    public Mock<IRegistrationService> RegistrationServiceMock { get; } = new Mock<IRegistrationService>();

    /// <summary>
    /// Gets the mock password recovery service.
    /// </summary>
    public Mock<IPasswordRecoveryService> PasswordRecoveryServiceMock { get; } = new Mock<IPasswordRecoveryService>();

    /// <summary>
    /// Gets the mock dashboard service.
    /// </summary>
    public Mock<IDashboardService> DashboardServiceMock { get; } = new Mock<IDashboardService>();

    /// <summary>
    /// Gets the mock profile service.
    /// </summary>
    public Mock<IProfileService> ProfileServiceMock { get; } = new Mock<IProfileService>();

    /// <summary>
    /// Configures the test server so the middleware pipeline runs end-to-end
    /// but all service-layer dependencies are replaced with mocks.
    /// </summary>
    /// <param name="builder">The web host builder.</param>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Provide the config values that AddInfrastructure requires so it
            // does not throw during startup. The real services are replaced below.
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=fake;Database=fake;",
                ["Jwt:Secret"] = "integration-test-secret-that-is-long-enough-for-hmac",
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove real infrastructure registrations and replace with mocks.
            ReplaceService<IJwtService>(services, JwtServiceMock.Object);
            ReplaceService<IAuthRepository>(services, AuthRepositoryMock.Object);
            ReplaceService<ILoginService>(services, LoginServiceMock.Object);
            ReplaceService<IRegistrationService>(services, RegistrationServiceMock.Object);
            ReplaceService<IPasswordRecoveryService>(services, PasswordRecoveryServiceMock.Object);
            ReplaceService<IDashboardService>(services, DashboardServiceMock.Object);
            ReplaceService<IProfileService>(services, ProfileServiceMock.Object);
        });
    }

    private static void ReplaceService<TService>(IServiceCollection services, TService implementation)
        where TService : class
    {
        ServiceDescriptor? existing = services.FirstOrDefault(d => d.ServiceType == typeof(TService));
        if (existing != null)
        {
            services.Remove(existing);
        }

        services.AddScoped(_ => implementation);
    }
}
