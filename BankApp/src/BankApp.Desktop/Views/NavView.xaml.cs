// <copyright file="NavView.xaml.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BankApp.Desktop.Master;
using BankApp.Desktop.Utilities;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace BankApp.Desktop.Views;

/// <summary>
/// Hosts the application shell after login: renders the sidebar and manages the inner content frame
/// where feature pages (Dashboard, Profile, etc.) are displayed.
/// </summary>
public sealed partial class NavView
{
    private readonly List<Button> navButtons;

    private Button? activeNavButton;

    private readonly IAppNavigationService navigationService;
    private readonly IApiClient apiClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="NavView"/> class.
    /// </summary>
    /// <param name="apiClient">Used to clear authentication state when the user logs out.</param>
    /// <param name="navigationService">Bound to the inner content frame to drive feature-page navigation.</param>
    public NavView(IApiClient apiClient, IAppNavigationService navigationService)
    {
        this.InitializeComponent();
        Current = this;
        this.navButtons =
        [
            this.NavDashboard, this.NavTransfers, this.NavBillPayments, this.NavCards,
            this.NavTransferHistory, this.NavCurrencyExchange, this.NavSavings,
            this.NavInvestments, this.NavStatistics, this.NavSupport, this.NavProfile
        ];
        this.apiClient = apiClient;
        this.navigationService = navigationService;
        this.navigationService.SetContentFrame(this.ContentFrame);
        this.navigationService.NavigateToContent<DashboardView>();
    }

    /// <summary>
    /// Gets the most recently created <see cref="NavView"/> instance.
    /// Used by content pages to call shell-level operations such as updating the notification badge.
    /// </summary>
    public static NavView? Current { get; private set; }

    /// <summary>
    /// Updates the notification badge on the bell icon to reflect the number of unread notifications.
    /// Hides the badge entirely when <paramref name="count"/> is zero or negative.
    /// </summary>
    /// <param name="count">The number of unread notifications to display.</param>
    public void UpdateNotificationBadge(int count)
    {
        if (count <= 0)
        {
            this.NotificationBadge.Visibility = Visibility.Collapsed;
            return;
        }

        this.NotificationBadgeText.Text = count > 99 ? "99+" : count.ToString();
        this.NotificationBadge.Visibility = Visibility.Visible;
    }

    /// <summary>
    /// Shows a modal dialog informing the user that the given feature is not yet available.
    /// Called by both sidebar buttons and content pages for unimplemented navigation targets.
    /// </summary>
    /// <param name="feature">The display name of the feature, shown as the dialog title and in the message body.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous dialog operation.</returns>
    public async Task ShowComingSoonAsync(string feature)
    {
        var dialog = new ContentDialog
        {
            Title = feature,
            Content = $"{feature} is coming soon.",
            CloseButtonText = "OK",
            XamlRoot = this.XamlRoot,
        };
        await dialog.ShowAsync();
    }

    private void SetActiveNav(Button selected)
    {
        foreach (var button in this.navButtons)
        {
            button.Style = (Style)this.Resources["NavItemStyle"];
        }

        selected.Style = (Style)this.Resources["NavItemActiveStyle"];
        this.activeNavButton = selected;
    }

    private void NavDashboard_Click(object sender, RoutedEventArgs e)
    {
        this.SetActiveNav(this.NavDashboard);
        this.navigationService.NavigateToContent<DashboardView>();
    }

    private void NavProfile_Click(object sender, RoutedEventArgs e)
    {
        this.SetActiveNav(this.NavProfile);
        this.navigationService.NavigateToContent<ProfileView>();
    }

    // All other nav items show a coming soon alert
    private async void NavTransfers_Click(object sender, RoutedEventArgs e) =>
        await this.ShowComingSoonAsync("Transfers");

    private async void NavBillPayments_Click(object sender, RoutedEventArgs e) =>
        await this.ShowComingSoonAsync("Bill Payments");

    private async void NavCards_Click(object sender, RoutedEventArgs e) =>
        await this.ShowComingSoonAsync("Cards");

    private async void NavTransferHistory_Click(object sender, RoutedEventArgs e) =>
        await this.ShowComingSoonAsync("Transfer History");

    private async void NavCurrencyExchange_Click(object sender, RoutedEventArgs e) =>
        await this.ShowComingSoonAsync("Currency Exchange");

    private async void NavSavings_Click(object sender, RoutedEventArgs e) =>
        await this.ShowComingSoonAsync("Savings & Loans");

    private async void NavInvestments_Click(object sender, RoutedEventArgs e) =>
        await this.ShowComingSoonAsync("Investments & Trading");

    private async void NavStatistics_Click(object sender, RoutedEventArgs e) =>
        await this.ShowComingSoonAsync("Statistics");

    private async void NavSupport_Click(object sender, RoutedEventArgs e) =>
        await this.ShowComingSoonAsync("Support");

    private void NotificationBell_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        var message = this.NotificationBadge.Visibility == Visibility.Visible
            ? $"You have {this.NotificationBadgeText.Text} unread notifications."
            : "You have no unread notifications.";

        _ = this.ShowAlertAsync("Notifications", message);
    }

    private async void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await this.apiClient.PostAsync<object>("/api/auth/logout", new { });
        }
        catch
        {
        }

        this.apiClient.ClearToken();
        this.navigationService.NavigateTo<LoginView>();
    }

    private async Task ShowAlertAsync(string title, string message)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = this.XamlRoot,
        };

        await dialog.ShowAsync();
    }
}
