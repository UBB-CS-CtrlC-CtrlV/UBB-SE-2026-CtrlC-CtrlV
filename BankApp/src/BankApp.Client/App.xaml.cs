// <copyright file="App.xaml.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankApp.Client.Master;
using BankApp.Client.Utilities;
using Microsoft.UI.Xaml;

namespace BankApp.Client
{
    public partial class App : Application
    {
        // Shared services, accessible from anywhere via App.ApiClient, App.NavigationService
        public static ApiClient ApiClient { get; } = new ApiClient();
        public static IAppNavigationService NavigationService { get; } = new AppNavigationService();

        private Window? m_window;

        public App()
        {
            this.InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            m_window.Activate();
        }
    }
}
