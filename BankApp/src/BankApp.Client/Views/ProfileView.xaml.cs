// <copyright file="ProfileView.xaml.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

using BankApp.Client.Enums;
using BankApp.Client.Master;
using BankApp.Client.Utilities;
using BankApp.Client.ViewModels;
using BankApp.Contracts.DTOs.Profile;
using BankApp.Contracts.Entities;
using BankApp.Contracts.Enums;
using BankApp.Contracts.Extensions;

namespace BankApp.Client.Views;

/// <summary>
/// Displays and manages the authenticated user's profile settings.
/// </summary>
public sealed partial class ProfileView : IStateObserver<ProfileState>
{
    private readonly ProfileViewModel viewModel;
    private string verifiedPassword = string.Empty;
    private string pendingTwoFactorAuthType = string.Empty;
    private bool isChangingPasswordFlow = false;
    private bool isTwoFactorFlow = false;
    private bool isUpdatingToggle = false;
    private readonly IAppNavigationService navigationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProfileView"/> class.
    /// </summary>
    /// <param name="viewModel">The view model that loads profile data and drives all profile update operations.</param>
    /// <param name="navigationService">Used to navigate to the dashboard or back to login.</param>
    public ProfileView(ProfileViewModel viewModel, IAppNavigationService navigationService)
    {
        this.InitializeComponent();
        this.viewModel = viewModel;
        this.navigationService = navigationService;
        this.viewModel.State.AddObserver(this);
        this.Loaded += this.OnPageLoaded;
    }

    private async void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        this.ShowLoading(true);
        await this.viewModel.LoadProfile();
        this.ShowLoading(false);
        this.PopulateUi();
        this.SetEditingEnabled(false);
    }

    /// <inheritdoc/>
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
    }

    /// <inheritdoc/>
    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        this.viewModel.State.RemoveObserver(this);
    }

    private void PopulateUi()
    {
        var user = this.viewModel.ProfileInfo;

        this.ProfileCardName.Text = user.FullName ?? string.Empty;
        this.ProfileCardEmail.Text = user.Email ?? string.Empty;
        this.ProfileCardPhone.Text = user.PhoneNumber ?? string.Empty;
        this.ProfileCardAddress.Text = user.Address ?? string.Empty;

        this.FullNameBox.Text = user.FullName ?? string.Empty;
        this.EmailBox.Text = user.Email ?? string.Empty;

        this.PhoneBox.Text = user.PhoneNumber ?? string.Empty;
        this.AddressBox.Text = user.Address ?? string.Empty;

        this.TwoFactorPhoneDisplay.Text = user.PhoneNumber ?? string.Empty;
        this.TwoFactorEmailDisplay.Text = user.Email ?? string.Empty;

        this.viewModel.IsInitializingView = true;
        this.TwoFactorToggle.IsOn = user.Is2FAEnabled;
        this.viewModel.IsInitializingView = false;

        this.PopulateOAuthLinks(this.viewModel.OAuth.OAuthLinks);
        this.PopulateNotificationPreferences(this.viewModel.Notifications.NotificationPreferences);
        this.Update2FaVisuals();
    }

    private void SetEditingEnabled(bool enabled)
    {
        this.PhoneBox.IsEnabled = enabled;
        this.AddressBox.IsEnabled = enabled;
        this.SaveButton.IsEnabled = enabled;

        this.PhoneBox.IsReadOnly = !enabled;
        this.AddressBox.IsReadOnly = !enabled;

        this.PhoneBox.Opacity = enabled ? 1.0 : 0.6;
        this.AddressBox.Opacity = enabled ? 1.0 : 0.6;

        if (!enabled)
        {
            return;
        }

        this.PhoneBox.Focus(FocusState.Programmatic);
        this.AddressBox.Focus(FocusState.Programmatic);
    }

    private async void UpdateButton_Click(object sender, RoutedEventArgs e)
    {
        this.isChangingPasswordFlow = false;
        this.isTwoFactorFlow = false;
        this.VerifyCurrentPasswordBox.Password = string.Empty;
        this.VerifyErrorInfoBar.IsOpen = false;
        await this.VerifyPasswordDialog.ShowAsync();
    }

    private async void VerifyPasswordDialog_PrimaryButtonClick(ContentDialog sender,
        ContentDialogButtonClickEventArgs args)
    {
        var deferral = args.GetDeferral();

        if (string.IsNullOrWhiteSpace(this.VerifyCurrentPasswordBox.Password))
        {
            this.VerifyErrorInfoBar.Message = "Enter your password.";
            this.VerifyErrorInfoBar.IsOpen = true;
            args.Cancel = true;
            deferral.Complete();
            return;
        }

        var verified = await this.viewModel.PersonalInfo.VerifyPassword(this.VerifyCurrentPasswordBox.Password);

        if (!verified)
        {
            this.VerifyErrorInfoBar.Message = "Incorrect password.";
            this.VerifyErrorInfoBar.IsOpen = true;
            args.Cancel = true;
            deferral.Complete();
            return;
        }

        this.verifiedPassword = this.VerifyCurrentPasswordBox.Password;
        this.VerifyErrorInfoBar.IsOpen = false;
        deferral.Complete();

        if (this.isChangingPasswordFlow)
        {
            this.DispatcherQueue.TryEnqueue(async void () =>
            {
                this.NewPasswordBox.Password = string.Empty;
                this.ConfirmPasswordBox.Password = string.Empty;
                this.NewPasswordErrorInfoBar.IsOpen = false;
                await this.NewPasswordDialog.ShowAsync();
            });
        }
        else if (this.isTwoFactorFlow)
        {
            this.DispatcherQueue.TryEnqueue(async void () =>
            {
                await this.Handle2FAActionAfterVerifyAsync();
            });
        }
        else
        {
            this.SetEditingEnabled(true);
            this.ShowSuccess("You can now edit your profile.");
        }
    }

    private async Task Handle2FAActionAfterVerifyAsync()
    {
        TwoFactorMethod method = this.pendingTwoFactorAuthType == "Phone"
            ? TwoFactorMethod.Phone
            : TwoFactorMethod.Email;

        bool success = await this.viewModel.Security.EnableTwoFactor(method);

        if (success)
        {
            this.viewModel.ProfileInfo.Is2FAEnabled = true;
            this.viewModel.ProfileInfo.Preferred2FAMethod = this.pendingTwoFactorAuthType;
            this.viewModel.IsInitializingView = true;
            this.TwoFactorToggle.IsOn = true;
            this.viewModel.IsInitializingView = false;
            this.Update2FaVisuals();
            this.ShowSuccess($"Two-factor authentication enabled via {this.pendingTwoFactorAuthType}.");
        }
        else
        {
            this.ShowError($"Failed to enable 2FA via {this.pendingTwoFactorAuthType}.");
        }
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        this.ShowLoading(true);

        var success = await this.viewModel.PersonalInfo.UpdatePersonalInfo(
            this.PhoneBox.Text,
            this.AddressBox.Text,
            this.verifiedPassword);

        this.ShowLoading(false);

        if (success)
        {
            this.ProfileCardPhone.Text = this.PhoneBox.Text.Trim();
            this.ProfileCardAddress.Text = this.AddressBox.Text.Trim();
            this.verifiedPassword = string.Empty;
            this.SetEditingEnabled(false);
            this.ShowSuccess("Profile updated successfully.");
        }
        else
        {
            this.ShowError("Failed to update profile.");
        }
    }

    private async void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
    {
        this.isChangingPasswordFlow = true;
        this.isTwoFactorFlow = false;
        this.VerifyCurrentPasswordBox.Password = string.Empty;
        this.VerifyErrorInfoBar.IsOpen = false;
        await this.VerifyPasswordDialog.ShowAsync();
    }

    private async void NewPasswordDialog_PrimaryButtonClick(ContentDialog sender,
        ContentDialogButtonClickEventArgs args)
    {
        var deferral = args.GetDeferral();

        var newPwd = this.NewPasswordBox.Password;
        var confirmPwd = this.ConfirmPasswordBox.Password;

        var userId = this.viewModel.ProfileInfo.UserId;
        if (userId == null)
        {
            this.NewPasswordErrorInfoBar.Message = "User not loaded.";
            this.NewPasswordErrorInfoBar.IsOpen = true;
            args.Cancel = true;
            deferral.Complete();
            return;
        }

        var (success, errorMessage) = await this.viewModel.Security.ChangePassword(
            userId.Value, this.verifiedPassword, newPwd, confirmPwd);

        if (success)
        {
            this.verifiedPassword = string.Empty;
            this.NewPasswordErrorInfoBar.IsOpen = false;
            deferral.Complete();
            this.ShowSuccess("Your password has been changed successfully.");
        }
        else
        {
            this.NewPasswordErrorInfoBar.Message = errorMessage;
            this.NewPasswordErrorInfoBar.IsOpen = true;
            args.Cancel = true;
            deferral.Complete();
        }
    }

    private async void Handle2FAAction_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        this.pendingTwoFactorAuthType = button?.Tag.ToString() ?? string.Empty;

        if (button?.Content.ToString() == "Remove")
        {
            bool success = await this.viewModel.Security.DisableTwoFactor();
            if (success)
            {
                this.viewModel.ProfileInfo.Is2FAEnabled = false;
                this.viewModel.ProfileInfo.Preferred2FAMethod = null;
                this.viewModel.IsInitializingView = true;
                this.TwoFactorToggle.IsOn = false;
                this.viewModel.IsInitializingView = false;
                this.Update2FaVisuals();
                this.ShowSuccess("Two-factor authentication has been disabled.");
            }
            else
            {
                this.ShowError("Failed to remove 2FA.");
            }
        }
        else
        {
            this.isTwoFactorFlow = true;
            this.isChangingPasswordFlow = false;
            this.VerifyCurrentPasswordBox.Password = string.Empty;
            this.VerifyErrorInfoBar.IsOpen = false;
            await this.VerifyPasswordDialog.ShowAsync();
        }
    }

    private async void TwoFactorToggle_Toggled(object sender, RoutedEventArgs e)
    {
        if (this.viewModel.IsInitializingView)
        {
            return;
        }

        var success = await this.viewModel.Security.SetTwoFactorEnabled(this.TwoFactorToggle.IsOn);

        if (!success)
        {
            this.viewModel.IsInitializingView = true;
            this.TwoFactorToggle.IsOn = !this.TwoFactorToggle.IsOn;
            this.viewModel.IsInitializingView = false;
            this.ShowError("Failed to update 2FA settings.");
        }
        else
        {
            this.viewModel.ProfileInfo.Is2FAEnabled = this.TwoFactorToggle.IsOn;
            this.Update2FaVisuals();
        }
    }

    private async void RemoveConnectedAccount_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: OAuthLinkDto link })
        {
            var success = await this.viewModel.OAuth.UnlinkOAuth(link.Provider);

            if (success)
            {
                this.PopulateOAuthLinks(this.viewModel.OAuth.OAuthLinks);
            }
            else
            {
                this.ShowError("Failed to remove account.");
            }
        }
    }

    private void ManageDevicesButton_Click(object sender, RoutedEventArgs e)
    {
    }

    private async void NotificationToggle_Toggled(object sender, RoutedEventArgs e)
    {
        if (this.viewModel.IsInitializingView)
        {
            return;
        }

        if (sender is ToggleSwitch { Tag: NotificationPreferenceDto preference } toggle)
        {
            this.isUpdatingToggle = true;
            await this.viewModel.Notifications.ToggleNotificationPreference(preference, toggle.IsOn);
            this.isUpdatingToggle = false;

            this.viewModel.IsInitializingView = true;
            toggle.IsOn = preference.EmailEnabled;
            this.viewModel.IsInitializingView = false;
        }
    }

    private void DashboardNavButton_Click(object sender, RoutedEventArgs e)
    {
        this.navigationService.NavigateTo<DashboardView>();
    }

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        this.navigationService.NavigateTo<LoginView>();
    }

    private void Update2FaVisuals()
    {
        var user = this.viewModel.ProfileInfo;
        this.TwoFactorPhoneDisplay.Text = this.viewModel.PersonalInfo.TwoFactorPhoneDisplay;

        bool phoneIsActive = user.Is2FAEnabled &&
            string.Equals(user.Preferred2FAMethod, "Phone", StringComparison.OrdinalIgnoreCase);
        bool emailIsActive = user.Is2FAEnabled &&
            string.Equals(user.Preferred2FAMethod, "Email", StringComparison.OrdinalIgnoreCase);

        if (!this.viewModel.PersonalInfo.HasPhoneNumber)
        {
            this.ConfigureActionButton(this.ActionPhoneBtn, this.PhoneStatusBadge,
                this.PhoneStatusText, "Add", "#F1F5F9", "#64748B", "Not configured");
        }
        else if (phoneIsActive)
        {
            this.ConfigureActionButton(this.ActionPhoneBtn, this.PhoneStatusBadge,
                this.PhoneStatusText, "Remove", "#DCFCE7", "#16A34A", "Active");
        }
        else
        {
            this.ConfigureActionButton(this.ActionPhoneBtn, this.PhoneStatusBadge,
                this.PhoneStatusText, "Verify", "#FFF7ED", "#C2410C", "Unverified");
        }

        if (emailIsActive)
        {
            this.ConfigureActionButton(this.ActionEmailBtn, this.EmailStatusBadge,
                this.EmailStatusText, "Remove", "#DCFCE7", "#16A34A", "Active");
        }
        else
        {
            this.ConfigureActionButton(this.ActionEmailBtn, this.EmailStatusBadge,
                this.EmailStatusText, "Verify", "#FFF7ED", "#C2410C", "Unverified");
        }
    }

    private void ConfigureActionButton(Button button, Border badge, TextBlock statusTxt,
        string action, string badgeBg, string textCol, string status)
    {
        button.Content = action;
        statusTxt.Text = status;
    }

    private void ShowLoading(bool visible)
    {
        this.LoadingPanel.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        this.LoadingRing.IsActive = visible;
        this.ErrorInfoBar.IsOpen = false;
        this.SuccessInfoBar.IsOpen = false;
    }

    private void ShowError(string message)
    {
        this.ErrorInfoBar.Message = message;
        this.ErrorInfoBar.IsOpen = true;
        this.SuccessInfoBar.IsOpen = false;
    }

    private void ShowSuccess(string message)
    {
        this.SuccessInfoBar.Message = message;
        this.SuccessInfoBar.IsOpen = true;
        this.ErrorInfoBar.IsOpen = false;
    }

    /// <inheritdoc/>
    public void Update(ProfileState state)
    {
        this.DispatcherQueue.TryEnqueue(() =>
        {
            if (this.isUpdatingToggle)
            {
                if (state == ProfileState.Error)
                {
                    this.ShowError("Failed to save notification preferences.");
                }

                return;
            }

            switch (state)
            {
                case ProfileState.Loading:
                    ShowLoading(true);
                    break;

                case ProfileState.UpdateSuccess:
                    ShowLoading(false);
                    this.PopulateUi();
                    break;

                case ProfileState.Error:
                    ShowLoading(false);
                    ShowError("Operation failed.");
                    break;
                case ProfileState.Idle:
                case ProfileState.Success:
                case ProfileState.PasswordChanged:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        });
    }

    private void TabPersonalBtn_Click(object sender, RoutedEventArgs e)
    {
        this.PanelPersonal.Visibility = Visibility.Visible;
        this.PanelSecurity.Visibility = Visibility.Collapsed;
        this.PanelNotifications.Visibility = Visibility.Collapsed;
        this.PanelSessions.Visibility = Visibility.Collapsed;

        this.TabPersonalBtn.Style = (Style)this.Resources["TabButtonActiveStyle"];
        this.TabSecurityBtn.Style = (Style)this.Resources["TabButtonStyle"];
        this.TabNotificationsBtn.Style = (Style)this.Resources["TabButtonStyle"];
        this.TabSessionsBtn.Style = (Style)this.Resources["TabButtonStyle"];
    }

    private void TabSecurityBtn_Click(object sender, RoutedEventArgs e)
    {
        this.PanelPersonal.Visibility = Visibility.Collapsed;
        this.PanelSecurity.Visibility = Visibility.Visible;
        this.PanelNotifications.Visibility = Visibility.Collapsed;
        this.PanelSessions.Visibility = Visibility.Collapsed;

        this.TabPersonalBtn.Style = (Style)this.Resources["TabButtonStyle"];
        this.TabSecurityBtn.Style = (Style)this.Resources["TabButtonActiveStyle"];
        this.TabNotificationsBtn.Style = (Style)this.Resources["TabButtonStyle"];
        this.TabSessionsBtn.Style = (Style)this.Resources["TabButtonStyle"];
    }

    private void TabNotificationsBtn_Click(object sender, RoutedEventArgs e)
    {
        this.PanelPersonal.Visibility = Visibility.Collapsed;
        this.PanelSecurity.Visibility = Visibility.Collapsed;
        this.PanelNotifications.Visibility = Visibility.Visible;
        this.PanelSessions.Visibility = Visibility.Collapsed;

        this.TabPersonalBtn.Style = (Style)this.Resources["TabButtonStyle"];
        this.TabSecurityBtn.Style = (Style)this.Resources["TabButtonStyle"];
        this.TabNotificationsBtn.Style = (Style)this.Resources["TabButtonActiveStyle"];
        this.TabSessionsBtn.Style = (Style)this.Resources["TabButtonStyle"];
    }

    private void PopulateOAuthLinks(List<OAuthLinkDto>? links)
    {
        this.OAuthLinksPanel.Children.Clear();

        if (links == null)
        {
            return;
        }

        foreach (var button in links.Select(link => new Button
        {
            Content = link.ProviderEmail ?? link.Provider,
            Tag = link,
        }))
        {
            button.Click += RemoveConnectedAccount_Click;
            this.OAuthLinksPanel.Children.Add(button);
        }
    }

    private void PopulateNotificationPreferences(List<NotificationPreferenceDto>? preferences)
    {
        this.viewModel.IsInitializingView = true;
        this.NotificationPreferencesPanel.Children.Clear();

        if (preferences == null)
        {
            this.viewModel.IsInitializingView = false;
            return;
        }

        foreach (var preference in preferences)
        {
            var row = new Grid
            {
                Margin = new Thickness(0, 6, 0, 6),
            };

            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var text = new TextBlock
            {
                Text = NotificationTypeExtensions.ToDisplayName(preference.Category),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 13,
                Foreground = (Brush)this.Resources["TextPrimary"],
            };

            var toggle = new ToggleSwitch
            {
                IsOn = preference.EmailEnabled,
                Tag = preference,
                VerticalAlignment = VerticalAlignment.Center,
            };

            toggle.Toggled += this.NotificationToggle_Toggled;

            Grid.SetColumn(text, 0);
            Grid.SetColumn(toggle, 1);

            row.Children.Add(text);
            row.Children.Add(toggle);

            this.NotificationPreferencesPanel.Children.Add(row);
        }

        this.viewModel.IsInitializingView = false;
    }

    private async void TabSessionsBtn_Click(object sender, RoutedEventArgs e)
    {
        this.PanelPersonal.Visibility = Visibility.Collapsed;
        this.PanelSecurity.Visibility = Visibility.Collapsed;
        this.PanelNotifications.Visibility = Visibility.Collapsed;
        this.PanelSessions.Visibility = Visibility.Visible;

        this.TabPersonalBtn.Style = (Style)this.Resources["TabButtonStyle"];
        this.TabSecurityBtn.Style = (Style)this.Resources["TabButtonStyle"];
        this.TabNotificationsBtn.Style = (Style)this.Resources["TabButtonStyle"];
        this.TabSessionsBtn.Style = (Style)this.Resources["TabButtonActiveStyle"];

        await this.LoadSessionsAsync();
    }

    private async Task LoadSessionsAsync()
    {
        this.SessionsErrorBar.IsOpen = false;
        this.SessionsSuccessBar.IsOpen = false;
        this.SessionsListPanel.Children.Clear();
        this.NoSessionsText.Visibility = Visibility.Collapsed;

        int? userId = this.viewModel.ProfileInfo.UserId;
        if (userId == null)
        {
            this.SessionsErrorBar.Message = "User not loaded.";
            this.SessionsErrorBar.IsOpen = true;
            return;
        }

        await this.viewModel.Sessions.LoadSessionsAsync(userId.Value);

        if (this.viewModel.Sessions.ActiveSessions.Count == 0)
        {
            this.NoSessionsText.Visibility = Visibility.Visible;
            return;
        }

        foreach (Session session in this.viewModel.Sessions.ActiveSessions)
        {
            this.SessionsListPanel.Children.Add(this.BuildSessionCard(session));
        }
    }

    private Border BuildSessionCard(Session session)
    {
        var card = new Border
        {
            Background = new SolidColorBrush(Microsoft.UI.Colors.White),
            CornerRadius = new CornerRadius(10),
            BorderBrush = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 226, 232, 240)),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(16, 12, 16, 12),
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var infoStack = new StackPanel { Spacing = 2 };

        var deviceText = new TextBlock
        {
            Text = session.DeviceInfo ?? "Unknown Device",
            FontSize = 13,
            Foreground = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 30, 41, 59)),
        };

        var browserText = new TextBlock
        {
            Text = session.Browser ?? "Unknown Browser",
            FontSize = 12,
            Foreground = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 100, 116, 139)),
        };

        var ipText = new TextBlock
        {
            Text = $"IP: {session.IpAddress ?? "Unknown"}",
            FontSize = 12,
            Foreground = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 100, 116, 139)),
        };

        var lastActiveText = new TextBlock
        {
            Text = session.LastActiveAt.HasValue
                ? $"Last active: {session.LastActiveAt.Value:g}"
                : "Last active: Unknown",
            FontSize = 11,
            Foreground = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 148, 163, 184)),
        };

        infoStack.Children.Add(deviceText);
        infoStack.Children.Add(browserText);
        infoStack.Children.Add(ipText);
        infoStack.Children.Add(lastActiveText);

        var revokeButton = new Button
        {
            Content = "Revoke",
            Tag = session.Id,
            VerticalAlignment = VerticalAlignment.Center,
        };
        revokeButton.Style = (Style)this.Resources["DangerButtonStyle"];
        revokeButton.Click += this.RevokeSessionButton_Click;

        Grid.SetColumn(infoStack, 0);
        Grid.SetColumn(revokeButton, 1);

        grid.Children.Add(infoStack);
        grid.Children.Add(revokeButton);

        card.Child = grid;
        return card;
    }

    private async void RevokeSessionButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: int sessionId })
        {
            return;
        }

        bool success = await this.viewModel.Sessions.RevokeSessionAsync(sessionId);

        if (success)
        {
            this.SessionsSuccessBar.Message = "Session revoked successfully.";
            this.SessionsSuccessBar.IsOpen = true;
            await this.LoadSessionsAsync();
        }
        else
        {
            this.SessionsErrorBar.Message = "Failed to revoke session.";
            this.SessionsErrorBar.IsOpen = true;
        }
    }
}
