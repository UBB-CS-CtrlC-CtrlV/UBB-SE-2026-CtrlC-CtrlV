// <copyright file="LoginView.xaml.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System;
using BankApp.Client.Master;
using BankApp.Client.Utilities;
using BankApp.Client.ViewModels;
using BankApp.Core.Enums;
using Microsoft.UI.Xaml;

namespace BankApp.Client.Views;

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
    public LoginView(LoginViewModel viewModel, IAppNavigationService navigationService)
    {
        this.navigationService = navigationService;
        this.InitializeComponent();

        this.viewModel = viewModel;
        this.viewModel.State.AddObserver(this);

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
                    break;

                case LoginState.Loading:
                    this.ShowLoading();
                    break;

                case LoginState.Success:
                    this.navigationService.NavigateTo<NavView>();
                    break;

                case LoginState.Require2Fa:
                    this.navigationService.NavigateTo<TwoFactorView>();
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

                case LoginState.ServerNotConfigured:
                    // TODO: consider making HideLoading either responsive to the ServerStatus or create a helper function that is so that this kind of state tricks are not needed
                    // HideLoading() re-enables buttons at the top of this method,
                    // so explicitly disable them again here to lock the form permanently.
                    this.SignInButton.IsEnabled = false;
                    this.GoogleLoginButton.IsEnabled = false;
                    this.ShowError("The app is not properly set up. Please contact your administrator.");
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
        this.navigationService.NavigateTo<ForgotPasswordView>();
    }

    private void CreateAccountButton_Click(object sender, RoutedEventArgs e)
    {
        this.navigationService.NavigateTo<RegisterView>();
    }
}