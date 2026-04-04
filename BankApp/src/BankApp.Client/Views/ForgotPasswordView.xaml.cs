// <copyright file="ForgotPasswordView.xaml.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System;
using BankApp.Client.Master;
using BankApp.Client.Utilities;
using BankApp.Client.ViewModels;
using BankApp.Core.Enums;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BankApp.Client.Views
{
    /// <summary>
    /// Displays the account recovery flow for requesting a reset code and setting a new password.
    /// This code-behind contains only UI-specific logic (loading state, message display, navigation).
    /// All business validation and state transitions are handled by <see cref="ForgotPasswordViewModel"/>.
    /// </summary>
    public sealed partial class ForgotPasswordView : IStateObserver<ForgotPasswordState>
    {
        private readonly ForgotPasswordViewModel viewModel;
        private readonly IAppNavigationService navigationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ForgotPasswordView"/> class.
        /// </summary>
        /// <param name="viewModel">The view model that drives password recovery logic and exposes recovery state.</param>
        /// <param name="navigationService">Used to navigate back to the login page after recovery completes.</param>
        public ForgotPasswordView(ForgotPasswordViewModel viewModel, IAppNavigationService navigationService)
        {
            this.InitializeComponent();

            this.viewModel = viewModel;
            this.navigationService = navigationService;
            this.viewModel.State.AddObserver(this);
        }

        /// <inheritdoc/>
        public void Update(ForgotPasswordState state)
        {
            this.OnStateChanged(state);
        }

        private void OnStateChanged(ForgotPasswordState state)
        {
            this.DispatcherQueue.TryEnqueue(() =>
            {
                this.HideLoading();

                if (state == ForgotPasswordState.Error && !string.IsNullOrWhiteSpace(this.viewModel.ValidationError))
                {
                    this.ShowMessage(this.viewModel.ValidationError, InfoBarSeverity.Warning);
                    return;
                }

                switch (state)
                {
                    case ForgotPasswordState.Idle:
                        this.Step1Panel.Visibility = Visibility.Visible;
                        this.Step2Panel.Visibility = Visibility.Collapsed;
                        break;

                    case ForgotPasswordState.EmailSent:
                        this.ShowMessage("A recovery code has been sent to your email.", InfoBarSeverity.Success);
                        this.InstructionText.Text = "Please paste the code from your email to continue.";
                        this.Step1Panel.Visibility = Visibility.Collapsed;
                        this.Step2Panel.Visibility = Visibility.Visible;
                        this.Step3Panel.Visibility = Visibility.Collapsed;
                        this.VerifyTokenButton.Visibility = Visibility.Visible;
                        this.ResendPanel.Visibility = Visibility.Visible;
                        this.TokenBox.IsEnabled = true;
                        this.TokenBox.Text = string.Empty;
                        break;

                    case ForgotPasswordState.PasswordResetSuccess:
                        this.ShowMessage("Your password has been reset successfully! You can now log in.", InfoBarSeverity.Success);
                        this.Step1Panel.Visibility = Visibility.Collapsed;
                        this.Step2Panel.Visibility = Visibility.Collapsed;
                        this.InstructionText.Text = "Account recovered successfully.";
                        break;

                    case ForgotPasswordState.TokenValid:
                        this.ShowMessage("Code verified! You can now set a new password.", InfoBarSeverity.Success);
                        this.VerifyTokenButton.Visibility = Visibility.Collapsed;
                        this.ResendPanel.Visibility = Visibility.Collapsed;
                        this.TokenBox.IsEnabled = false;
                        this.Step3Panel.Visibility = Visibility.Visible;
                        break;

                    case ForgotPasswordState.TokenExpired:
                        this.ShowMessage("The recovery code has expired. Please request a new one.", InfoBarSeverity.Error);
                        break;

                    case ForgotPasswordState.TokenAlreadyUsed:
                        this.ShowMessage("This recovery code has already been used.", InfoBarSeverity.Error);
                        break;

                    case ForgotPasswordState.Error:
                        this.ShowMessage("An error occurred. Please try again.", InfoBarSeverity.Error);
                        break;

                    default:
                        throw new System.ArgumentOutOfRangeException(nameof(state), state, null);
                }
            });
        }

        private async void SendCodeButton_Click(object sender, RoutedEventArgs e)
        {
            this.StatusInfoBar.IsOpen = false;
            this.ShowLoading();
            await this.viewModel.ForgotPassword(this.EmailBox.Text.Trim());
        }

        private async void ResetPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            this.StatusInfoBar.IsOpen = false;
            this.ShowLoading();
            await this.viewModel.ResetPassword(this.NewPasswordBox.Password, this.TokenBox.Text.Trim());
        }

        private async void VerifyTokenButton_Click(object sender, RoutedEventArgs e)
        {
            this.StatusInfoBar.IsOpen = false;
            this.ShowLoading();
            await this.viewModel.VerifyToken(this.TokenBox.Text.Trim());
        }

        private async void ResendCodeButton_Click(object sender, RoutedEventArgs e)
        {
            this.StatusInfoBar.IsOpen = false;
            this.ShowLoading();
            await this.viewModel.ForgotPassword(this.EmailBox.Text.Trim());
        }

        private void BackToLoginButton_Click(object sender, RoutedEventArgs e)
        {
            this.navigationService.NavigateTo<LoginView>();
        }

        /// <summary>
        /// Shows a status message in the view's info bar.
        /// </summary>
        /// <param name="msg">The message to display.</param>
        /// <param name="severity">The severity level.</param>
        private void ShowMessage(string msg, InfoBarSeverity severity)
        {
            this.StatusInfoBar.Message = msg;
            this.StatusInfoBar.Severity = severity;
            this.StatusInfoBar.IsOpen = true;
        }

        private void ShowLoading()
        {
            this.LoadingRing.IsActive = true;
            this.LoadingRing.Visibility = Visibility.Visible;
            this.SendCodeButton.IsEnabled = false;
            this.ResetPasswordButton.IsEnabled = false;
        }

        private void HideLoading()
        {
            this.LoadingRing.IsActive = false;
            this.LoadingRing.Visibility = Visibility.Collapsed;
            this.SendCodeButton.IsEnabled = true;
            this.ResetPasswordButton.IsEnabled = true;
        }
    }
}
