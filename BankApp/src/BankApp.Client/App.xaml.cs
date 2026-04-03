// <copyright file="App.xaml.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankApp.Client.Master;
using BankApp.Client.Utilities;
using Microsoft.UI.Xaml;

namespace BankApp.Client
{
    /// <summary>
    /// Provides application startup and shared client services.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Shared services, accessible from anywhere via App.ApiClient.
        /// </summary>
        public static ApiClient ApiClient { get; } = new ApiClient();

        /// <summary>
        /// Shared services, accessible from anywhere via App.NavigationService.
        /// </summary>
        public static IAppNavigationService NavigationService { get; } = new AppNavigationService();

        private Window? window;

        /// <summary>
        /// TODO: add docs.
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// TODO: add docs.
        /// </summary>
        /// <param name="args"></param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            this.window = new MainWindow();
            this.window.Activate();
        }
    }
}
