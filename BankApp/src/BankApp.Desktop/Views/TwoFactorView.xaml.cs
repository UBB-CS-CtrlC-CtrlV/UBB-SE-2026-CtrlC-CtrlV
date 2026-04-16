// <copyright file="TwoFactorView.xaml.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;
using BankApp.Desktop.Master;
using BankApp.Desktop.Utilities;
using BankApp.Desktop.ViewModels;
using BankApp.Desktop.Enums;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BankApp.Desktop.Views;

/// <summary>
/// Displays the OTP verification step of the login flow.
/// This file contains only UI-layer concerns: navigation, dialog management, and
/// the Visibility conversion helper used by compiled <c>{x:Bind}</c> expressions.
/// All business logic (timer countdown, OTP validation, state transitions) lives in
/// <see cref="TwoFactorViewModel"/>.
/// </summary>
public sealed partial class TwoFactorView : Page, IStateObserver<TwoFactorState>
{
    private readonly TwoFactorViewModel viewModel;
    private readonly IAppNavigationService navigationService;
    private readonly IApiClient apiClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="TwoFactorView"/> class.
    /// </summary>
    /// <param name="viewModel">The view model that drives OTP verification logic and exposes two-factor state.</param>
    /// <param name="navigationService">Used to navigate to other pages in response to state changes.</param>
    /// <param name="apiClient">Used to clear authentication state when the user cancels and returns to login.</param>
    public TwoFactorView(TwoFactorViewModel viewModel, IAppNavigationService navigationService, IApiClient apiClient)
    {
        this.InitializeComponent();

        this.viewModel = viewModel;
        this.navigationService = navigationService;
        this.apiClient = apiClient;
        this.viewModel.State.AddObserver(this);
    }

    /// <summary>
    /// Gets the view model. Exposed as a public property so that compiled
    /// <c>{x:Bind ViewModel.Property}</c> expressions in the XAML can resolve it.
    /// </summary>
    public TwoFactorViewModel ViewModel => this.viewModel;

    /// <inheritdoc/>
    public void Update(TwoFactorState state)
    {
        this.DispatcherQueue.TryEnqueue(() => this.OnStateChanged(state));
    }

    // ─── Visibility helper for {x:Bind} function expressions ──────────────────
    // Used in XAML as: Visibility="{x:Bind BoolToVisibility(ViewModel.SomeBool), Mode=OneWay}"
    // Keeps WinUI-specific Visibility type out of the ViewModel.
    private Visibility BoolToVisibility(bool value) =>
        value ? Visibility.Visible : Visibility.Collapsed;

    // ─── State handling ────────────────────────────────────────────────────────

    /// <summary>
    /// Reacts to state transitions from the ViewModel.
    /// Most visual state is handled automatically through {x:Bind} — this method
    /// only handles cases that require imperative navigation calls.
    /// </summary>
    private void OnStateChanged(TwoFactorState state)
    {
        switch (state)
        {
            case TwoFactorState.Success:
                this.navigationService.NavigateTo<NavView>();
                break;

            case TwoFactorState.Idle:
            case TwoFactorState.Verifying:
            case TwoFactorState.InvalidOTP:
            case TwoFactorState.Expired:
            case TwoFactorState.MaxAttemptsReached:
                // Handled through bindings: HasError, ErrorMessage, IsInputEnabled.
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }

    // ─── Event handlers ────────────────────────────────────────────────────────
    private async void VerifyButton_Click(object sender, RoutedEventArgs e)
    {
        // Validation (6-digit length check) is enforced inside the ViewModel.
        await this.viewModel.VerifyOtp();
    }

    private async void ResendButton_Click(object sender, RoutedEventArgs e)
    {
        // Guard against premature resend is enforced inside the ViewModel.
        await this.viewModel.ResendOtp();
    }

    /// <summary>
    /// Propagates the typed text to the ViewModel so that <see cref="TwoFactorViewModel.OtpCode"/>
    /// is always in sync without requiring a Two-Way binding.
    /// </summary>
    private void OtpBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        this.viewModel.OtpCode = this.OtpBox.Text;
    }

    private void BackToLoginButton_Click(object sender, RoutedEventArgs e)
    {
        this.apiClient.ClearToken();
        this.navigationService.NavigateTo<LoginView>();
    }
}
