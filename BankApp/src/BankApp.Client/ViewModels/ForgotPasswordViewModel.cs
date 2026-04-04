// <copyright file="ForgotPasswordViewModel.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;
using BankApp.Client.Utilities;
using BankApp.Core.Enums;

namespace BankApp.Client.ViewModels;

/// <summary>
/// Coordinates the forgot-password flow by delegating all business logic to
/// <see cref="IPasswordRecoveryManager"/> and exposing observable state to the View.
/// </summary>
public class ForgotPasswordViewModel
{
    private readonly IPasswordRecoveryManager recoveryManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="ForgotPasswordViewModel"/> class
    /// using a real <see cref="ApiClient"/> and the production system clock.
    /// </summary>
    /// <param name="apiClient">The HTTP client used for API calls.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="apiClient"/> is null.</exception>
    public ForgotPasswordViewModel(ApiClient apiClient)
        : this(new PasswordRecoveryManager(
            apiClient ?? throw new ArgumentNullException(nameof(apiClient)),
            new SystemClock()))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ForgotPasswordViewModel"/> class
    /// with an explicit recovery manager (for testing).
    /// </summary>
    /// <param name="recoveryManager">The recovery manager to delegate to.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="recoveryManager"/> is null.</exception>
    public ForgotPasswordViewModel(IPasswordRecoveryManager recoveryManager)
    {
        this.recoveryManager = recoveryManager ?? throw new ArgumentNullException(nameof(recoveryManager));
        this.State = new ObservableState<ForgotPasswordState>(ForgotPasswordState.Idle);
    }

    /// <summary>
    /// Gets the observable state of the forgot-password flow.
    /// </summary>
    public ObservableState<ForgotPasswordState> State { get; }

    /// <summary>
    /// Gets a value indicating whether the user is currently allowed to resend a recovery code.
    /// </summary>
    public bool CanResendCode => this.recoveryManager.CanResendCode;

    /// <summary>
    /// Gets the seconds remaining before the user may request another recovery code.
    /// </summary>
    public int SecondsUntilResendAllowed => this.recoveryManager.SecondsUntilResendAllowed;

    /// <summary>
    /// Gets any pending validation error message.  Set before transitioning state so the
    /// View can display it without needing its own validation logic.
    /// </summary>
    public string ValidationError { get; private set; } = string.Empty;

    /// <summary>
    /// Requests a password-recovery code for the given email address.
    /// </summary>
    /// <param name="email">The email address to send the code to.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task ForgotPassword(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            this.ValidationError = "Please enter your email address.";
            this.State.SetValue(ForgotPasswordState.Error);
            return;
        }

        this.ValidationError = string.Empty;
        var newState = await this.recoveryManager.RequestCodeAsync(email);
        this.State.SetValue(newState);
    }

    /// <summary>
    /// Validates and resets the password using the supplied token.
    /// </summary>
    /// <param name="newPassword">The new password chosen by the user.</param>
    /// <param name="code">The reset token received by email.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task ResetPassword(string newPassword, string code)
    {
        if (string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(code))
        {
            this.ValidationError = "Please fill in all fields.";
            this.State.SetValue(ForgotPasswordState.Error);
            return;
        }

        if (!this.recoveryManager.IsPasswordValid(newPassword))
        {
            this.ValidationError = "Password must be at least 8 characters with uppercase, lowercase, a digit, and a special character.";
            this.State.SetValue(ForgotPasswordState.Error);
            return;
        }

        this.ValidationError = string.Empty;
        var newState = await this.recoveryManager.ResetPasswordAsync(code, newPassword);
        this.State.SetValue(newState);
    }

    /// <summary>
    /// Verifies whether the supplied reset token is still valid.
    /// </summary>
    /// <param name="code">The token to check.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task VerifyToken(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            this.ValidationError = "Please paste the recovery code first.";
            this.State.SetValue(ForgotPasswordState.Error);
            return;
        }

        this.ValidationError = string.Empty;
        var newState = await this.recoveryManager.VerifyTokenAsync(code);
        this.State.SetValue(newState);
    }
}
