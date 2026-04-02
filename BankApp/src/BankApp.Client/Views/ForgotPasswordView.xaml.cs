// <copyright file="ForgotPasswordView.xaml.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;
using BankApp.Client.Utilities;
using BankApp.Client.ViewModels;
using BankApp.Core.Enums;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BankApp.Client.Views
{
    /// <summary>
    /// TODO: add docs.
    /// </summary>
    public sealed partial class ForgotPasswordView : Page, IStateObserver<ForgotPasswordState>
    {
        private readonly ForgotPasswordViewModel viewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="ForgotPasswordView"/> class.
        /// </summary>
        public ForgotPasswordView()
        {
            this.InitializeComponent();

            this.viewModel = new ForgotPasswordViewModel(App.ApiClient);
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
                        throw new ArgumentOutOfRangeException(nameof(state), state, null);
                }
            });
        }

        private async Task SendCodeButton_Click(object sender, RoutedEventArgs e)
        {
            this.StatusInfoBar.IsOpen = false;
            var email = this.EmailBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(email))
            {
                this.ShowMessage("Please enter your email address.", InfoBarSeverity.Warning);
                return;
            }

            this.ShowLoading();
            await this.viewModel.ForgotPassword(email);
        }

        private async Task ResetPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            this.StatusInfoBar.IsOpen = false;
            var code = this.TokenBox.Text.Trim();
            var newPassword = this.NewPasswordBox.Password;
            var confirmPassword = this.ConfirmPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(newPassword))
            {
                this.ShowMessage("Please fill in all fields.", InfoBarSeverity.Warning);
                return;
            }

            if (newPassword != confirmPassword)
            {
                this.ShowMessage("Passwords do not match.", InfoBarSeverity.Warning);
                return;
            }

            if (newPassword.Length < 8 ||
                !System.Linq.Enumerable.Any(newPassword, char.IsUpper) ||
                !System.Linq.Enumerable.Any(newPassword, char.IsLower) ||
                !System.Linq.Enumerable.Any(newPassword, char.IsDigit) ||
                !System.Linq.Enumerable.Any(newPassword, ch => !char.IsLetterOrDigit(ch)))
            {
                this.ShowMessage("Password must be at least 8 characters with uppercase, lowercase, a digit, and a special character.", InfoBarSeverity.Warning);
                return;
            }

            this.ShowLoading();
            await this.viewModel.ResetPassword(this.EmailBox.Text.Trim(), newPassword, code);
        }

        private async Task VerifyTokenButton_Click(object sender, RoutedEventArgs e)
        {
            this.StatusInfoBar.IsOpen = false;
            var code = this.TokenBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(code))
            {
                this.ShowMessage("Please paste the recovery code first.", InfoBarSeverity.Warning);
                return;
            }

            this.ShowLoading();
            await this.viewModel.VerifyToken(code);
        }

        private async Task ResendCodeButton_Click(object sender, RoutedEventArgs e)
        {
            this.StatusInfoBar.IsOpen = false;
            var email = this.EmailBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(email))
            {
                this.ShowMessage("Email is missing. Please go back to login and try again.", InfoBarSeverity.Error);
                return;
            }

            this.ShowLoading();

            await this.viewModel.ForgotPassword(email);
        }

        private void BackToLoginButton_Click(object sender, RoutedEventArgs e)
        {
            App.NavigationService.NavigateTo<LoginView>();
        }

        /// <summary>
        /// TODO: add documentation.
        /// </summary>
        /// <param name="msg">The message.</param>
        /// <param name="severity">The severity.</param>
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
