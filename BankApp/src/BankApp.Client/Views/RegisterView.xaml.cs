// <copyright file="RegisterView.xaml.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankApp.Client.Master;
using BankApp.Client.Utilities;
using BankApp.Client.ViewModels;
using BankApp.Client.Enums;
using Microsoft.UI.Xaml;

namespace BankApp.Client.Views;

/// <summary>
/// Displays the registration form and reacts to registration state changes.
/// </summary>
public sealed partial class RegisterView : IStateObserver<RegisterState>
{
    private readonly RegisterViewModel viewModel;
    private readonly IAppNavigationService navigationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterView"/> class.
    /// </summary>
    /// <param name="viewModel">The view model that drives registration logic and exposes registration state.</param>
    /// <param name="navigationService">Used to navigate to other pages in response to state changes.</param>
    public RegisterView(RegisterViewModel viewModel, IAppNavigationService navigationService)
    {
        this.InitializeComponent();

        this.viewModel = viewModel;
        this.navigationService = navigationService;
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
                    this.navigationService.NavigateTo<NavView>();
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
        this.navigationService.NavigateTo<LoginView>();
    }
}
