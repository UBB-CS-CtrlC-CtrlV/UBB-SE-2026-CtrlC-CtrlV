using Microsoft.UI.Xaml;
using BankApp.Client.Utilities;
using BankApp.Client.Master;

namespace BankApp.Client
{
    public partial class App : Application
    {
        // Shared services, accessible from anywhere via App.ApiService, App.NavigationService
        public static ApiService ApiService { get; private set; } = new ApiService();
        public static NavigationService NavigationService { get; private set; } = new NavigationService();

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