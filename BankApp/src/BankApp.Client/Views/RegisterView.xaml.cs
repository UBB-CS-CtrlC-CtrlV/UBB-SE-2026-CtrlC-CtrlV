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
    private readonly IRegistrationContext registrationContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterView"/> class.
    /// </summary>
    /// <param name="viewModel">The view model that drives registration logic and exposes registration state.</param>
    /// <param name="navigationService">Used to navigate to other pages in response to state changes.</param>
    /// <param name="registrationContext">Carries the just-registered flag to the login page.</param>
    public RegisterView(RegisterViewModel viewModel, IAppNavigationService navigationService, IRegistrationContext registrationContext)
    {
        this.InitializeComponent();

        this.viewModel = viewModel;
        this.navigationService = navigationService;
        this.registrationContext = registrationContext;
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

            switch (state)
            {
                case RegisterState.Loading:
                    this.ShowLoading();
                    break;

                case RegisterState.Success:
                    this.registrationContext.JustRegistered = true;
                    this.navigationService.NavigateTo<LoginView>();
                    break;

                case RegisterState.AutoLoggedIn:
                    this.navigationService.NavigateTo<NavView>();
                    break;

                case RegisterState.EmailAlreadyExists:
                    this.ShowError(UserMessages.Register.EmailAlreadyExists);
                    break;

                case RegisterState.InvalidEmail:
                    this.ShowError(UserMessages.Register.InvalidEmail);
                    break;

                case RegisterState.WeakPassword:
                    this.ShowError(UserMessages.Register.WeakPassword);
                    break;

                case RegisterState.PasswordMismatch:
                    this.ShowError(UserMessages.Register.PasswordMismatch);
                    break;

                case RegisterState.Error:
                    this.ShowError(UserMessages.Register.AllFieldsRequired);
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

    private async void RegisterButton_Click(object sender, RoutedEventArgs e)
    {
        await this.viewModel.Register(
            this.EmailBox.Text,
            this.PasswordBox.Password,
            this.ConfirmPasswordBox.Password,
            this.FullNameBox.Text);
    }

    private async void GoogleRegisterButton_Click(object sender, RoutedEventArgs e)
    {
        await this.viewModel.OAuthRegister("Google");
    }

    private void BackToLoginButton_Click(object sender, RoutedEventArgs e)
    {
        this.navigationService.NavigateTo<LoginView>();
    }
}
