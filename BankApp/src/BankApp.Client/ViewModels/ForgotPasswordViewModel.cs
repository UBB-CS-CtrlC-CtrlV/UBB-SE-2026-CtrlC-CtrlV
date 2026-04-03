// <copyright file="ForgotPasswordViewModel.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;
using BankApp.Client;
using BankApp.Client.Utilities;
using BankApp.Core.DTOs.Auth;
using BankApp.Core.Enums;

namespace BankApp.Client.ViewModels;

/// <summary>
/// Loads and exposes the data needed by the forgot-password flow.
/// </summary>
public class ForgotPasswordViewModel
{
    private readonly ApiClient apiClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="ForgotPasswordViewModel"/> class.
    /// </summary>
    /// <param name="apiClient">The api client.</param>
    /// <exception cref="ArgumentNullException">When the api client is null.</exception>
    public ForgotPasswordViewModel(ApiClient apiClient)
    {
        this.apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        this.State = new ObservableState<ForgotPasswordState>(ForgotPasswordState.Idle);
    }

    /// <summary>
    /// Gets the state.
    /// </summary>
    public ObservableState<ForgotPasswordState> State { get; private set; }

    /// <summary>
    /// Requests a password reset code for the specified email address.
    /// </summary>
    /// <param name="email">The email address associated with the account.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    public async Task ForgotPassword(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            this.State.SetValue(ForgotPasswordState.Error);
            return;
        }

        try
        {
            var request = new ForgotPasswordRequest { Email = email };
            var response = await this.apiClient.PostAsync<ForgotPasswordRequest, ApiResponse>("/api/auth/forgot-password", request);
            this.State.SetValue(
                response is { error: null } ? ForgotPasswordState.EmailSent : ForgotPasswordState.Error);
        }
        catch (Exception)
        {
            this.State.SetValue(ForgotPasswordState.Error);
        }
    }

    /// <summary>
    /// Resets the password for a previously verified reset token.
    /// </summary>
    /// <param name="newPassword">The new password to apply to the account.</param>
    /// <param name="code">The reset token provided to the user.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    public async Task ResetPassword(string newPassword, string code)
    {
        if (string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(code))
        {
            this.State.SetValue(ForgotPasswordState.Error);
            return;
        }

        try
        {
            var request = new ResetPasswordRequest
            {
                Token = code,
                NewPassword = newPassword,
            };
            var response = await this.apiClient.PostAsync<ResetPasswordRequest, ApiResponse>("/api/auth/reset-password", request);
            this.State.SetValue(this.MapResetTokenState(response, ForgotPasswordState.PasswordResetSuccess));
        }
        catch (Exception)
        {
            this.State.SetValue(ForgotPasswordState.Error);
        }
    }

    /// <summary>
    /// Verifies whether the supplied reset token can still be used.
    /// </summary>
    /// <param name="code">The reset token to validate.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task VerifyToken(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            this.State.SetValue(ForgotPasswordState.Error);
            return;
        }

        try
        {
            var response = await this.apiClient.PostAsync<object, ApiResponse>("/api/auth/verify-reset-token", new { Token = code });

            this.State.SetValue(this.MapResetTokenState(response, ForgotPasswordState.TokenValid));
        }
        catch (Exception)
        {
            this.State.SetValue(ForgotPasswordState.Error);
        }
    }

    private ForgotPasswordState MapResetTokenState(ApiResponse? response, ForgotPasswordState successState)
    {
        if (response is { error: null })
        {
            return successState;
        }

        return response?.errorCode switch
        {
            "token_expired" => ForgotPasswordState.TokenExpired,
            "token_already_used" => ForgotPasswordState.TokenAlreadyUsed,
            "token_invalid" => ForgotPasswordState.TokenInvalid,
            _ => ForgotPasswordState.Error,
        };
    }
}
