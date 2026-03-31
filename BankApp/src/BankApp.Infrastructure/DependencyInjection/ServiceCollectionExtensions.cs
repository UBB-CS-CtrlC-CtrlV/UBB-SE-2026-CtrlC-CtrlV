using BankApp.Infrastructure.DataAccess;
using BankApp.Infrastructure.DataAccess.Implementations;
using BankApp.Infrastructure.DataAccess.Interfaces;
using BankApp.Infrastructure.Repositories.Implementations;
using BankApp.Infrastructure.Repositories.Interfaces;
using BankApp.Infrastructure.Services.Implementations;
using BankApp.Infrastructure.Services.Infrastructure.Implementations;
using BankApp.Infrastructure.Services.Infrastructure.Interfaces;
using BankApp.Infrastructure.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BankApp.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");
        string jwtSecret = configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("Configuration value 'Jwt:Secret' is missing.");

        services.AddScoped<AppDbContext>(_ => new AppDbContext(connectionString));

        services.AddScoped<IUserDAO, UserDAO>();
        services.AddScoped<ISessionDAO, SessionDAO>();
        services.AddScoped<IOAuthLinkDAO, OAuthLinkDAO>();
        services.AddScoped<IPasswordResetTokenDAO, PasswordResetTokenDAO>();
        services.AddScoped<INotificationPreferenceDAO, NotificationPreferenceDAO>();
        services.AddScoped<IAccountDAO, AccountDAO>();
        services.AddScoped<ICardDAO, CardDAO>();
        services.AddScoped<ITransactionDAO, TransactionDAO>();
        services.AddScoped<INotificationDAO, NotificationDAO>();

        services.AddScoped<IHashService, HashService>();
        services.AddScoped<IJWTService>(_ => new JWTService(jwtSecret));
        services.AddScoped<IOTPService, OTPService>();
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
