// <copyright file="ProfileView.xaml.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using BankApp.Client.Master;
using BankApp.Client.Utilities;
using BankApp.Client.ViewModels;
using BankApp.Contracts.DTOs.Profile;
using BankApp.Client.Enums;
using BankApp.Contracts.Enums;
using BankApp.Contracts.Extensions;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

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
    }

    /// <inheritdoc/>
    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        this.ShowLoading(true);

        await this.viewModel.LoadProfile();

        this.ShowLoading(false);

        this.PopulateUi();

        this.SetEditingEnabled(false);
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
        this.isChangingPasswordFlow = false; // Just editing info
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

        // Success logic
        this.verifiedPassword = this.VerifyCurrentPasswordBox.Password;
        this.VerifyErrorInfoBar.IsOpen = false;

        // Complete the deferral so the FIRST dialog closes
        deferral.Complete();

        // Now, trigger the NEXT step based on the flow
        if (this.isChangingPasswordFlow)
        {
            // We MUST use the Dispatcher to wait until the first dialog is gone
            this.DispatcherQueue.TryEnqueue(async void () =>
            {
                this.NewPasswordBox.Password = string.Empty;
                this.ConfirmPasswordBox.Password = string.Empty;
                this.NewPasswordErrorInfoBar.IsOpen = false;
                await this.NewPasswordDialog.ShowAsync();
            });
        }
        else if (!this.isTwoFactorFlow)
        {
            // Normal profile edit flow
            this.SetEditingEnabled(true);
            this.ShowSuccess("You can now edit your profile.");
        }
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        this.ShowLoading(true);

        var success = await this.viewModel.PersonalInfo.UpdatePersonalInfo(this.PhoneBox.Text,
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
        this.isChangingPasswordFlow = true; // Password change flow
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

        var (success, errorMessage) = await this.viewModel.Security.ChangePassword(userId.Value, this.verifiedPassword, newPwd, confirmPwd);

        if (success)
        {
            this.verifiedPassword = string.Empty; // Clear security sensitive data
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
        this.pendingTwoFactorAuthType = button?.Tag.ToString() ?? string.Empty; // "Phone" or "Email"

        if (button?.Content.ToString() == "Remove")
        {
            // Logic for removal
        }
        else
        {
            // Logic for Add/Verify
            this.isTwoFactorFlow = true;
            this.VerifyCurrentPasswordBox.Password = string.Empty;
            await this.VerifyPasswordDialog.ShowAsync();
        }
    }

    private async void SaveTwoFactorSettings_Click(object sender, RoutedEventArgs e)
    {
        // bool success = await _viewModel.UpdateTwoFactorContacts(
        //    TwoFactorPhoneBox.Text.Trim(),
        //    TwoFactorEmailBox.Text.Trim());

        // if (success)
        //    ShowSuccess("2FA settings saved.");
        // else
        //    ShowError("Failed to save 2FA settings.");
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
            this.ShowError("Failed to update 2FA settings");
        }
        else
        {
            this.viewModel.ProfileInfo.Is2FAEnabled = this.TwoFactorToggle.IsOn;
        }
    }

    private async void TwoFactorEmailToggle_Toggled(object sender, RoutedEventArgs e)
    {
        // bool success = TwoFactorEmailToggle.IsOn
        //    ? await _viewModel.EnableTwoFactor(TwoFactorMethod.Email)
        //    : await _viewModel.DisableTwoFactor(TwoFactorMethod.Email);

        // if (!success)
        //    ShowError("2FA email update failed.");
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
        this.TwoFactorPhoneDisplay.Text = this.viewModel.PersonalInfo.TwoFactorPhoneDisplay;

        if (!this.viewModel.PersonalInfo.HasPhoneNumber)
        {
            this.ConfigureActionButton(this.ActionPhoneBtn, PhoneStatusBadge, PhoneStatusText, "Add", "#F1F5F9",
                "#64748B", "Disabled");
        }
        else
        {
            this.ConfigureActionButton(this.ActionPhoneBtn, PhoneStatusBadge, PhoneStatusText, "Verify", "#FFF7ED",
                "#C2410C", "Unverified");
        }
    }

    private void ConfigureActionButton(Button button, Border badge, TextBlock statusTxt, string action, string badgeBg,
        string textCol, string status)
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
            // --- INTERCEPTOR: Block full-page reloads if we are just toggling a switch ---
            if (this.isUpdatingToggle)
            {
                if (state == ProfileState.Error)
                {
                    this.ShowError("Failed to save notification preferences.");
                }

                // Ignore Loading and UpdateSuccess so the screen doesn't wipe and redraw!
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

        this.TabPersonalBtn.Style = (Style)this.Resources["TabButtonActiveStyle"];
        this.TabSecurityBtn.Style = (Style)this.Resources["TabButtonStyle"];
        this.TabNotificationsBtn.Style = (Style)this.Resources["TabButtonStyle"];
    }

    private void TabSecurityBtn_Click(object sender, RoutedEventArgs e)
    {
        this.PanelPersonal.Visibility = Visibility.Collapsed;
        this.PanelSecurity.Visibility = Visibility.Visible;
        this.PanelNotifications.Visibility = Visibility.Collapsed;

        this.TabPersonalBtn.Style = (Style)this.Resources["TabButtonStyle"];
        this.TabSecurityBtn.Style = (Style)this.Resources["TabButtonActiveStyle"];
        this.TabNotificationsBtn.Style = (Style)this.Resources["TabButtonStyle"];
    }

    private void TabNotificationsBtn_Click(object sender, RoutedEventArgs e)
    {
        this.PanelPersonal.Visibility = Visibility.Collapsed;
        this.PanelSecurity.Visibility = Visibility.Collapsed;
        this.PanelNotifications.Visibility = Visibility.Visible;

        this.TabPersonalBtn.Style = (Style)this.Resources["TabButtonStyle"];
        this.TabSecurityBtn.Style = (Style)this.Resources["TabButtonStyle"];
        this.TabNotificationsBtn.Style = (Style)this.Resources["TabButtonActiveStyle"];
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
}