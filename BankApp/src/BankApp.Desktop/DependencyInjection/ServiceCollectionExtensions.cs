// <copyright file="ServiceCollectionExtensions.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankApp.Desktop.Master;
using BankApp.Desktop.Utilities;
using BankApp.Desktop.ViewModels;
using BankApp.Desktop.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BankApp.Desktop.DependencyInjection;

/// <summary>
/// Provides extension methods for registering client-side services
/// with the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all client-side services and view models with the service collection.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">
    /// The application configuration built from <c>appsettings.json</c>,
    /// <c>appsettings.Local.json</c>, and environment variables. Registered as a
    /// singleton so all services and view models can receive it via constructor injection.
    /// </param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddClientServices(this IServiceCollection services, IConfiguration configuration)
    {
        // IConfiguration is registered as a singleton so IApiClient and ViewModels can
        // receive it via constructor injection without reaching back into the composition root.
        services.AddSingleton<IConfiguration>(configuration);

        // One HttpClient instance must be shared for the entire
        // application lifetime to avoid socket exhaustion.
        services.AddSingleton<IApiClient, ApiClient>();

        // Navigation state must be the same
        // object throughout the application.
        // Multiple instances would lose the frame reference.
        services.AddSingleton<IAppNavigationService, AppNavigationService>();
        services.AddSingleton<IRegistrationContext, RegistrationContext>();

        // A fresh timer instance per TwoFactorView so each page visit
        // has its own independent countdown.
        services.AddTransient<ICountdownTimer, DispatcherCountdownTimer>();

        // Each navigation creates a fresh ViewModel.
        // No stale state leaks between page visits.
        services.AddTransient<LoginViewModel>();
        services.AddTransient<RegisterViewModel>();
        services.AddTransient<TwoFactorViewModel>();
        services.AddTransient<ForgotPasswordViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<PersonalInfoViewModel>();
        services.AddTransient<SecurityViewModel>();
        services.AddTransient<OAuthViewModel>(); // To Do: Change to OAuth
        services.AddTransient<NotificationsViewModel>();
        services.AddTransient<SessionsViewModel>();
        services.AddTransient<ProfileViewModel>();

        // Views are registered as transient so the navigation service can resolve them
        // through the container. Each navigation gets a fresh page instance with all
        // constructor dependencies (ViewModels, NavigationService) injected automatically.
        services.AddTransient<LoginView>();
        services.AddTransient<RegisterView>();
        services.AddTransient<TwoFactorView>();
        services.AddTransient<ForgotPasswordView>();
        services.AddTransient<NavView>(); // To Do: Change to Nav
        services.AddTransient<DashboardView>();
        services.AddTransient<ProfileView>();

        return services;
    }
}
