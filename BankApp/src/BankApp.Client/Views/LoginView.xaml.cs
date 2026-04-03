// <copyright file="LoginView.xaml.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System;
using BankApp.Client.Utilities;
using BankApp.Client.ViewModels;
using BankApp.Core.Enums;
using Microsoft.UI.Xaml;

namespace BankApp.Client.Views
{
    /// <summary>
    /// Display the login view.
    /// </summary>
    public sealed partial class LoginView : IStateObserver<LoginState>
    {
        private readonly LoginViewModel viewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginView"/> class.
        /// </summary>
        public LoginView()
        {
            this.InitializeComponent();

            this.viewModel = new LoginViewModel(App.ApiClient);
            this.viewModel.State.AddObserver(this);
        }

        /// <inheritdoc/>
        public void Update(LoginState state)
        {
            this.OnStateChanged(state);
        }

        private void OnStateChanged(LoginState state)
        {
            this.DispatcherQueue.TryEnqueue(() =>
            {
                this.HideLoading();
                this.ErrorInfoBar.IsOpen = false;

                switch (state)
                {
                    case LoginState.Idle:
                        break;

                    case LoginState.Loading:
                        this.ShowLoading();
                        break;

                    case LoginState.Success:
                        App.NavigationService.NavigateTo<NavView>(); // goes to NavView
                        break;

                    case LoginState.Require2FA:
                        App.NavigationService.NavigateTo<TwoFactorView>();
                        break;

                    case LoginState.InvalidCredentials:
                        this.ShowError("Invalid email or password.");
                        break;

                    case LoginState.AccountLocked:
                        this.ShowError("Account is locked. Try again later.");
                        break;

                    case LoginState.Error:
                        this.ShowError("Something went wrong. Please try again.");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(state), state, null);
                }
            });
        }

        private void ShowError(string msg)
        {
            this.ErrorInfoBar.Message = msg;
            this.ErrorInfoBar.IsOpen = true;
        }

        private void ShowLoading()
        {
            this.LoadingRing.IsActive = true;
            this.LoadingRing.Visibility = Visibility.Visible;
            this.SignInButton.IsEnabled = false;
        }

        private void HideLoading()
        {
            this.LoadingRing.IsActive = false;
            this.LoadingRing.Visibility = Visibility.Collapsed;
            this.SignInButton.IsEnabled = true;
        }

        private async void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            var email = this.EmailBox.Text.Trim();
            var password = this.PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                this.ShowError("Please enter email and password.");
                return;
            }

            await this.viewModel.Login(email, password);
        }

        private async void GoogleLoginButton_Click(object sender, RoutedEventArgs e)
        {
            await this.viewModel.OAuthLogin("Google");
        }

        private void ForgotPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            App.NavigationService.NavigateTo<ForgotPasswordView>();
        }

        private void CreateAccountButton_Click(object sender, RoutedEventArgs e)
        {
            App.NavigationService.NavigateTo<RegisterView>();
        }
    }
}
