// <copyright file="App.xaml.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System;
using BankApp.Client.DependencyInjection;
using BankApp.Client.Master;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace BankApp.Client;

/// <summary>
/// Composition root for the client application.
/// </summary>
public partial class App
{
    /// <summary>
    /// Gets the application-wide DI container.
    /// Only resolve services at the composition root boundary (i.e. in <see cref="OnLaunched"/>).
    /// All other classes must receive their dependencies via constructor injection.
    /// </summary>
    public IServiceProvider Services { get; }

    private Window? window;

    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class
    /// and builds the dependency injection container.
    /// </summary>
    public App()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            // appsettings.Local.json is `.gitignored`
            // and only exists in dev environments.
            // It overrides appsettings.json locally.
            .AddJsonFile("appsettings.Local.json", optional: true)
            // Environment variables are the final override layer for CI/Prod builds.
            .AddEnvironmentVariables()
            .Build();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddClientServices(configuration);
        this.Services = serviceCollection.BuildServiceProvider();
        this.InitializeComponent();
    }

    /// <summary>
    /// Invoked when the application is launched. Resolves the navigation service,
    /// creates the main window and activates it.
    /// </summary>
    /// <param name="args">
    /// Contains information about the launch request and process, such as the
    /// activation kind and previous execution state.
    /// </param>
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var navigationService = this.Services.GetRequiredService<IAppNavigationService>();
        this.window = new MainWindow(navigationService);
        this.window.Activate();
    }
}