// <copyright file="NavView.xaml.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace BankApp.Client.Views;

/// <summary>
/// Display the navigation view.
/// </summary>
public sealed partial class NavView
{
    private readonly List<Button> navButtons;

    private Button? activeNavButton;

    /// <summary>
    /// Initializes a new instance of the <see cref="NavView"/> class.
    /// </summary>
    public NavView()
    {
        this.InitializeComponent();
        Current = this;
        this.navButtons =
        [
            this.NavDashboard, this.NavTransfers, this.NavBillPayments, this.NavCards,
            this.NavTransferHistory, this.NavCurrencyExchange, this.NavSavings,
            this.NavInvestments, this.NavStatistics, this.NavSupport, this.NavProfile
        ];
        App.NavigationService.SetContentFrame(this.ContentFrame);
        App.NavigationService.NavigateToContent<DashboardView>();
    }

    /// <summary>
    /// Gets tODO: add docs.
    /// </summary>
    public static NavView? Current { get; private set; }

    /// <summary>
    /// TODO: add docs.
    /// </summary>
    /// <param name="count">The count.</param>
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
    /// TODO: add docs.
    /// </summary>
    /// <param name="feature">The feature.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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
        foreach (var btn in this.navButtons)
        {
            btn.Style = (Style)this.Resources["NavItemStyle"];
        }

        selected.Style = (Style)this.Resources["NavItemActiveStyle"];
        this.activeNavButton = selected;
    }

    private void NavDashboard_Click(object sender, RoutedEventArgs e)
    {
        this.SetActiveNav(this.NavDashboard);
        App.NavigationService.NavigateToContent<DashboardView>();
    }

    private void NavProfile_Click(object sender, RoutedEventArgs e)
    {
        this.SetActiveNav(this.NavProfile);
        App.NavigationService.NavigateToContent<ProfileView>();
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

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        // await App.ApiClient.PostAsync("/api/auth/logout", null);
        App.ApiClient.ClearToken();
        App.NavigationService.NavigateTo<LoginView>();
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
