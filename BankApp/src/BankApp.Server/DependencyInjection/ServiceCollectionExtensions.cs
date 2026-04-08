using BankApp.Server.DataAccess;
using BankApp.Server.DataAccess.Implementations;
using BankApp.Server.DataAccess.Interfaces;
using BankApp.Server.Repositories.Implementations;
using BankApp.Server.Repositories.Interfaces;
using BankApp.Server.Services.Auth;
using BankApp.Server.Services.Notifications;
using BankApp.Server.Services.Security;
using BankApp.Server.Services.Dashboard;
using BankApp.Server.Services.Profile;

namespace BankApp.Server.DependencyInjection;

/// <summary>
/// Provides extension methods for registering infrastructure services with the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all infrastructure services, data access components, and repositories with the service collection.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The application configuration used to resolve connection strings and secrets.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the <c>DefaultConnection</c> connection string or <c>Jwt:Secret</c> configuration value is missing.
    /// </exception>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");
        string jwtSecret = configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("Configuration value 'Jwt:Secret' is missing.");

        services.AddScoped<AppDbContext>(_ => new AppDbContext(connectionString));

        services.AddScoped<IUserDataAccess, UserDataAccess>();
        services.AddScoped<ISessionDataAccess, SessionDataAccess>();
        services.AddScoped<IOAuthLinkDataAccess, OAuthLinkDataAccess>();
        services.AddScoped<IPasswordResetTokenDataAccess, PasswordResetTokenDataAccess>();
        services.AddScoped<INotificationPreferenceDataAccess, NotificationPreferenceDataAccess>();
        services.AddScoped<IAccountDataAccess, AccountDataAccess>();
        services.AddScoped<ICardDataAccess, CardDataAccess>();
        services.AddScoped<ITransactionDataAccess, TransactionDataAccess>();
        services.AddScoped<INotificationDataAccess, NotificationDataAccess>();

        services.AddScoped<IHashService, HashService>();
        services.AddScoped<IJwtService>(_ => new JwtService(jwtSecret));
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<IEmailService, EmailService>();

        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IDashboardRepository, DashboardRepository>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IDashboardService, DashboardService>();

        return services;
    }
}

