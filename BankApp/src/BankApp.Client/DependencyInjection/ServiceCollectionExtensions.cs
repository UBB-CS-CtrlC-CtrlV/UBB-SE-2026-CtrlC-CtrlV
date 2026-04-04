// <copyright file="ServiceCollectionExtensions.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankApp.Client.Master;
using BankApp.Client.Utilities;
using BankApp.Client.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace BankApp.Client.DependencyInjection;

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
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddClientServices(this IServiceCollection services)
    {
        // One HttpClient instance must be shared for the entire
        // application lifetime to avoid socket exhaustion.
        services.AddSingleton<ApiClient>();

        // Navigation state must be the same
        // object throughout the app.
        // Multiple instances would lose the frame reference.
        services.AddSingleton<IAppNavigationService, AppNavigationService>();

        // Each navigation creates a fresh ViewModel.
        // No stale state leaks between page visits.
        services.AddTransient<LoginViewModel>();
        services.AddTransient<RegisterViewModel>();
        services.AddTransient<TwoFactorViewModel>();
        services.AddTransient<ForgotPasswordViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<ProfileViewModel>();

        return services;
    }
}
