using Microsoft.UI.Xaml;
using BankApp.Client.Utilities;
using BankApp.Client.Master;

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
