// <copyright file="BankAppWebFactory.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankApp.Server.Repositories.Interfaces;
using BankApp.Server.Services.Dashboard;
using BankApp.Server.Services.Login;
using BankApp.Server.Services.PasswordRecovery;
using BankApp.Server.Services.Profile;
using BankApp.Server.Services.Registration;
using BankApp.Server.Services.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace BankApp.Server.Tests.Integration.Infrastructure;

/// <summary>
/// A custom <see cref="WebApplicationFactory{TEntryPoint}"/> that replaces all
/// infrastructure services with NSubstitute stubs so integration tests can exercise the
/// full HTTP pipeline without a database or external dependencies.
/// </summary>
public class BankAppWebFactory : WebApplicationFactory<Program>
{
    /// <summary>
    /// Gets the substitute JWT service that controls token validation behaviour.
    /// </summary>
    public IJwtService JwtServiceSub { get; } = SubstituteFactory.CreateJwtService();

    /// <summary>
    /// Gets the substitute auth repository that controls session lookup behaviour.
    /// </summary>
    public IAuthRepository AuthRepositorySub { get; } = SubstituteFactory.CreateAuthRepository();

    /// <summary>
    /// Gets the substitute login service.
    /// </summary>
    public ILoginService LoginServiceSub { get; } = SubstituteFactory.CreateLoginService();

    /// <summary>
    /// Gets the substitute registration service.
    /// </summary>
    public IRegistrationService RegistrationServiceSub { get; } = SubstituteFactory.CreateRegistrationService();

    /// <summary>
    /// Gets the substitute password recovery service.
    /// </summary>
    public IPasswordRecoveryService PasswordRecoveryServiceSub { get; } = SubstituteFactory.CreatePasswordRecoveryService();

    /// <summary>
    /// Gets the substitute dashboard service.
    /// </summary>
    public IDashboardService DashboardServiceSub { get; } = SubstituteFactory.CreateDashboardService();

    /// <summary>
    /// Gets the substitute profile service.
    /// </summary>
    public IProfileService ProfileServiceSub { get; } = SubstituteFactory.CreateProfileService();

    /// <summary>
    /// Configures the test server so the middleware pipeline runs end-to-end
    /// but all service-layer dependencies are replaced with substitutes.
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
            // Remove real infrastructure registrations and replace with substitutes.
            ReplaceService<IJwtService>(services, this.JwtServiceSub);
            ReplaceService<IAuthRepository>(services, this.AuthRepositorySub);
            ReplaceService<ILoginService>(services, this.LoginServiceSub);
            ReplaceService<IRegistrationService>(services, this.RegistrationServiceSub);
            ReplaceService<IPasswordRecoveryService>(services, this.PasswordRecoveryServiceSub);
            ReplaceService<IDashboardService>(services, this.DashboardServiceSub);
            ReplaceService<IProfileService>(services, this.ProfileServiceSub);
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
