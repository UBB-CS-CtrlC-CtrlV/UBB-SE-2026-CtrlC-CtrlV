using BankApp.Client.Utilities;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;

namespace BankApp.Client.Views
{
    public sealed partial class NavView : Page
    {
        public static NavView? Current { get; private set; }

        private Button? _activeNavButton;

        private readonly List<Button> _navButtons;

        public NavView()
        {
            this.InitializeComponent();
            Current = this;
            _navButtons = new List<Button>
            {
                NavDashboard, NavTransfers, NavBillPayments, NavCards,
                NavTransferHistory, NavCurrencyExchange, NavSavings,
                NavInvestments, NavStatistics, NavSupport, NavProfile
            };
            App.NavigationService.SetContentFrame(ContentFrame);
            App.NavigationService.NavigateToContent<DashboardView>();
        }

        private void SetActiveNav(Button selected)
        {
            foreach (Button btn in _navButtons)
                btn.Style = (Style)Resources["NavItemStyle"];
            selected.Style = (Style)Resources["NavItemActiveStyle"];
            _activeNavButton = selected;
        }
        
        public void UpdateNotificationBadge(int count)
        {
            if (count <= 0)
            {
                NotificationBadge.Visibility = Visibility.Collapsed;
                return;
            }
            NotificationBadgeText.Text = count > 99 ? "99+" : count.ToString();
            NotificationBadge.Visibility = Visibility.Visible;
        }

        private void NavDashboard_Click(object sender, RoutedEventArgs e)
        {
            SetActiveNav(NavDashboard);
            App.NavigationService.NavigateToContent<DashboardView>();
        }

        private void NavProfile_Click(object sender, RoutedEventArgs e)
        {
            SetActiveNav(NavProfile);
            App.NavigationService.NavigateToContent<ProfileView>();
        }

        // All other nav items show a coming soon alert
        private async void NavTransfers_Click(object sender, RoutedEventArgs e) =>
            await ShowComingSoonAsync("Transfers");

        private async void NavBillPayments_Click(object sender, RoutedEventArgs e) =>
            await ShowComingSoonAsync("Bill Payments");

        private async void NavCards_Click(object sender, RoutedEventArgs e) =>
            await ShowComingSoonAsync("Cards");

        private async void NavTransferHistory_Click(object sender, RoutedEventArgs e) =>
            await ShowComingSoonAsync("Transfer History");

        private async void NavCurrencyExchange_Click(object sender, RoutedEventArgs e) =>
            await ShowComingSoonAsync("Currency Exchange");

        private async void NavSavings_Click(object sender, RoutedEventArgs e) =>
            await ShowComingSoonAsync("Savings & Loans");

        private async void NavInvestments_Click(object sender, RoutedEventArgs e) =>
            await ShowComingSoonAsync("Investments & Trading");

        private async void NavStatistics_Click(object sender, RoutedEventArgs e) =>
            await ShowComingSoonAsync("Statistics");

        private async void NavSupport_Click(object sender, RoutedEventArgs e) =>
            await ShowComingSoonAsync("Support");

        public async System.Threading.Tasks.Task ShowComingSoonAsync(string feature)
        {
            var dialog = new ContentDialog
            {
                Title = feature,
                Content = $"{feature} is coming soon.",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }

        private void NotificationBell_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var message = NotificationBadge.Visibility == Visibility.Visible
                ? $"You have {NotificationBadgeText.Text} unread notifications."
                : "You have no unread notifications.";

            _ = this.ShowAlertAsync("Notifications", message);
        }

        private async void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            //await App.ApiClient.PostAsync("/api/auth/logout", null);
            App.ApiClient.ClearToken();
            App.NavigationService.NavigateTo<LoginView>();
        }

        private async System.Threading.Tasks.Task ShowAlertAsync(string title, string message)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }
    }
}
