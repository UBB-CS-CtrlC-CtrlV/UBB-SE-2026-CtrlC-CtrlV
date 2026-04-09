// <copyright file="App.xaml.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System;
using System.IO;
using BankApp.Client.DependencyInjection;
using BankApp.Client.Master;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Serilog;

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
    private IServiceProvider Services { get; }

    private Window? window;

    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class
    /// and builds the dependency injection container.
    /// </summary>
    public App()
    {
        ConfigureLogging();

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            // appsettings.Local.json is `.gitignore`
            // and only exists in dev environments.
            // It overrides appsettings.json locally.
            .AddJsonFile("appsettings.Local.json", optional: true)
            // Environment variables are the final override layer for CI/Prod builds.
            .AddEnvironmentVariables()
            .Build();

        var serviceCollection = new ServiceCollection();

        // AddLogging registers ILoggerFactory and ILogger<T> in the container.
        // AddSerilog bridges the MEL abstraction to the Serilog backend configured above.
        // dispose: true ensures Serilog flushes when the container is disposed.
        serviceCollection.AddLogging(logging => logging.AddSerilog(dispose: true));

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

    private static void ConfigureLogging()
    {
        string logDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "BankApp",
            "Logs");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            // Writes to the Visual Studio Output window during development.
            .WriteTo.Debug()
            // Writes to a daily rolling file outside the repo.
            // Log path: %LocalAppData%\BankApp\Logs\bankapp-client-YYYYMMDD.log
            .WriteTo.File(
                path: Path.Combine(logDirectory, "bankapp-client-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14)
            .CreateLogger();
    }
}
