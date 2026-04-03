// <copyright file="RegisterView.xaml.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankApp.Client.Utilities;
using BankApp.Client.ViewModels;
using BankApp.Core.Enums;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BankApp.Client.Views
{
    /// <summary>
    /// Displays the registration flow for creating a new account.
    /// </summary>
    public sealed partial class RegisterView : Page, IStateObserver<RegisterState>
    {
        private readonly RegisterViewModel viewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterView"/> class.
        /// </summary>
        public RegisterView()
        {
            this.InitializeComponent();

            this.viewModel = new RegisterViewModel(App.ApiClient);
            this.viewModel.State.AddObserver(this);
        }

        /// <inheritdoc/>
        public void Update(RegisterState state)
        {
            this.OnStateChanged(state);
        }

        private void OnStateChanged(RegisterState state)
        {
            this.DispatcherQueue.TryEnqueue(() =>
            {
                this.HideLoading();
                this.ErrorInfoBar.IsOpen = false;
                this.SuccessInfoBar.IsOpen = false;

                switch (state)
                {
                    case RegisterState.Idle:
                        break;

                    case RegisterState.Loading:
                        this.ShowLoading();
                        break;

                    case RegisterState.Success:
                        this.SuccessInfoBar.IsOpen = true;
                        this.ClearForm();
                        break;

                    case RegisterState.AutoLoggedIn:
                        App.NavigationService.NavigateTo<NavView>();
                        break;

                    case RegisterState.EmailAlreadyExists:
                        this.ShowError("This email is already registered.");
                        break;

                    case RegisterState.InvalidEmail:
                        this.ShowError("Please enter a valid email address.");
                        break;

                    case RegisterState.WeakPassword:
                        this.ShowError("Password must be at least 8 characters with uppercase, lowercase, a digit and a special character.");
                        break;

                    case RegisterState.PasswordMismatch:
                        this.ShowError("Passwords do not match.");
                        break;

                    case RegisterState.Error:
                        this.ShowError("Something went wrong. Please try again.");
                        break;

                    default:
                        break;
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
            this.RegisterButton.IsEnabled = false;
        }

        private void HideLoading()
        {
            this.LoadingRing.IsActive = false;
            this.LoadingRing.Visibility = Visibility.Collapsed;
            this.RegisterButton.IsEnabled = true;
        }

        private void ClearForm()
        {
            this.FullNameBox.Text = string.Empty;
            this.EmailBox.Text = string.Empty;
            this.PasswordBox.Password = string.Empty;
            this.ConfirmPasswordBox.Password = string.Empty;
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string fullName = this.FullNameBox.Text.Trim();
            string email = this.EmailBox.Text.Trim();
            string password = this.PasswordBox.Password;
            string confirmPassword = this.ConfirmPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(fullName)
                || string.IsNullOrWhiteSpace(email)
                || string.IsNullOrWhiteSpace(password)
                || string.IsNullOrWhiteSpace(confirmPassword))
            {
                this.ShowError("Please fill in all fields.");
                return;
            }

            await this.viewModel.Register(email, password, confirmPassword, fullName);
        }

        private async void GoogleRegisterButton_Click(object sender, RoutedEventArgs e)
        {
            await this.viewModel.OAuthRegister("Google");
        }

        private void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            App.NavigationService.NavigateTo<LoginView>();
        }
    }
}
