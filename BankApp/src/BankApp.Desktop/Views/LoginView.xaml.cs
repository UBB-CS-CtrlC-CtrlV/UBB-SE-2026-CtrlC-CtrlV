// <copyright file="LoginView.xaml.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System;
using BankApp.Desktop.Master;
using BankApp.Desktop.Utilities;
using BankApp.Desktop.ViewModels;
using BankApp.Desktop.Enums;
using Microsoft.UI.Xaml;

namespace BankApp.Desktop.Views;

/// <summary>
/// Displays the login form and reacts to authentication state changes produced by <see cref="LoginViewModel"/>.
/// </summary>
public sealed partial class LoginView : IStateObserver<LoginState>
{
    private readonly LoginViewModel viewModel;
    private readonly IAppNavigationService navigationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginView"/> class.
    /// </summary>
    /// <param name="viewModel">The view model that drives authentication logic and exposes login state.</param>
    /// <param name="navigationService">Used to navigate to other pages in response to state changes.</param>
    /// <param name="registrationContext">Carries the just-registered flag set by the register page.</param>
    public LoginView(LoginViewModel viewModel, IAppNavigationService navigationService, IRegistrationContext registrationContext)
    {
        this.navigationService = navigationService;
        this.InitializeComponent();

        this.viewModel = viewModel;
        this.viewModel.State.AddObserver(this);

        if (registrationContext.JustRegistered)
        {
            registrationContext.JustRegistered = false;
            this.RegistrationSuccessBar.IsOpen = true;
        }

        // Apply the ViewModel's current state immediately. The ViewModel is constructed
        // before the view subscribes, so any state set in the constructor (e.g.
        // ServerNotConfigured when ApiBaseUrl is missing) would otherwise be missed.
        this.OnStateChanged(this.viewModel.State.Value);
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
                    this.EnableForm();
                    break;

                case LoginState.Loading:
                    this.ShowLoading();
                    break;

                case LoginState.Success:
                    this.EnableForm();
                    this.navigationService.NavigateTo<NavView>();
                    break;

                case LoginState.Require2Fa:
                    this.EnableForm();
                    this.navigationService.NavigateTo<TwoFactorView>();
                    break;

                case LoginState.InvalidCredentials:
                    this.EnableForm();
                    this.ShowError("Invalid email or password.");
                    break;

                case LoginState.AccountLocked:
                    this.EnableForm();
                    this.ShowError("Account is locked. Try again later.");
                    break;

                case LoginState.Error:
                    this.EnableForm();
                    this.ShowError("Something went wrong. Please try again.");
                    break;

                case LoginState.ServerNotConfigured:
                    // Form stays disabled — misconfiguration cannot be resolved at runtime.
                    this.ShowError("The application is not properly set up. Please contact your administrator.");
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        });
    }

    private void ShowError(string message)
    {
        this.ErrorInfoBar.Message = message;
        this.ErrorInfoBar.IsOpen = true;
    }

    private void EnableForm()
    {
        this.SignInButton.IsEnabled = true;
        this.GoogleLoginButton.IsEnabled = true;
    }

    private void ShowLoading()
    {
        this.LoadingRing.IsActive = true;
        this.LoadingRing.Visibility = Visibility.Visible;
        this.SignInButton.IsEnabled = false;
        this.GoogleLoginButton.IsEnabled = false;
    }

    private void HideLoading()
    {
        this.LoadingRing.IsActive = false;
        this.LoadingRing.Visibility = Visibility.Collapsed;
    }

    private async void SignInButton_Click(object sender, RoutedEventArgs e)
    {
        var email = this.EmailBox.Text;
        var password = this.PasswordBox.Password;

        if (!this.viewModel.CanLogin(email, password))
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
        this.navigationService.NavigateTo<ForgotPasswordView>();
    }

    private void CreateAccountButton_Click(object sender, RoutedEventArgs e)
    {
        this.navigationService.NavigateTo<RegisterView>();
    }
}