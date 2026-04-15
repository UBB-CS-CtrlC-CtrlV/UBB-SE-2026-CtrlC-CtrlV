// <copyright file="SecurityViewModel.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;
using BankApp.Client.Utilities;
using BankApp.Contracts.DTOs.Profile;
using BankApp.Client.Enums;
using BankApp.Contracts.Enums;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace BankApp.Client.ViewModels;

/// <summary>
/// Handles security-related profile operations such as password changes
/// and two-factor authentication management.
/// </summary>
public class SecurityViewModel
{
    private readonly ApiClient apiClient;
    private readonly ILogger<SecurityViewModel> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SecurityViewModel"/> class.
    /// </summary>
    /// <param name="apiClient">The API client used for security operations.</param>
    /// <param name="logger">Logger for security operation errors.</param>
    public SecurityViewModel(ApiClient apiClient, ILogger<SecurityViewModel> logger)
    {
        this.apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.State = new ObservableState<ProfileState>(ProfileState.Idle);
    }

    /// <summary>
    /// Gets the current security workflow state.
    /// </summary>
    public ObservableState<ProfileState> State { get; }

    /// <summary>
    /// Enables or disables two-factor authentication for the current user.
    /// </summary>
    /// <param name="enabled">
    /// <see langword="true"/> to enable 2FA via email; <see langword="false"/> to disable it.
    /// </param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    public async Task<bool> SetTwoFactorEnabled(bool enabled)
    {
        return enabled
            ? await this.EnableTwoFactor(TwoFactorMethod.Email)
            : await this.DisableTwoFactor();
    }

    /// <summary>
    /// Changes the current user's password.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="currentPassword">The current password for verification.</param>
    /// <param name="newPassword">The new password to apply.</param>
    /// <param name="confirmPassword">The password confirmation.</param>
    /// <returns>A tuple indicating success and an optional error message.</returns>
    public async Task<(bool Success, string ErrorMessage)> ChangePassword(int userId, string currentPassword, string newPassword, string confirmPassword)
    {
        if (!PasswordValidator.MeetsMinimumLength(newPassword))
        {
            return (false, UserMessages.Security.MinimumLengthRequired);
        }

        if (newPassword != confirmPassword)
        {
            return (false, UserMessages.Security.PasswordMismatch);
        }

        this.State.SetValue(ProfileState.Loading);

        ChangePasswordRequest request = new ChangePasswordRequest(userId, currentPassword, newPassword);
        ErrorOr<Success> result = await this.apiClient.PutAsync<ChangePasswordRequest>(ApiEndpoints.ChangePassword, request);

        return result.Match(
            _ =>
            {
                this.State.SetValue(ProfileState.UpdateSuccess);
                return (true, string.Empty);
            },
            errors =>
            {
                this.logger.LogError("ChangePassword failed: {Errors}", errors);
                this.State.SetValue(ProfileState.Error);
                string message = errors[0].Code == "incorrect_password"
                    ? UserMessages.Security.IncorrectPassword
                    : UserMessages.Security.UnexpectedError;
                return (false, message);
            });
    }

    /// <summary>
    /// Enables two-factor authentication for the current user.
    /// </summary>
    /// <param name="method">The two-factor delivery method to enable.</param>
    /// <returns><see langword="true"/> if the setting was updated; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> EnableTwoFactor(TwoFactorMethod method)
    {
        this.State.SetValue(ProfileState.Loading);

        object request = new { Method = method };
        ErrorOr<Success> result = await this.apiClient.PutAsync<object>(ApiEndpoints.Enable2Fa, request);

        return result.Match(
            _ =>
            {
                this.State.SetValue(ProfileState.UpdateSuccess);
                return true;
            },
            errors =>
            {
                this.logger.LogError("EnableTwoFactor failed: {Errors}", errors);
                this.State.SetValue(ProfileState.Error);
                return false;
            });
    }

    /// <summary>
    /// Disables two-factor authentication for the current user.
    /// </summary>
    /// <returns><see langword="true"/> if the setting was updated; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> DisableTwoFactor()
    {
        this.State.SetValue(ProfileState.Loading);

        ErrorOr<Success> result = await this.apiClient.PutAsync<object>(ApiEndpoints.Disable2Fa, new { });

        return result.Match(
            _ =>
            {
                this.State.SetValue(ProfileState.UpdateSuccess);
                return true;
            },
            errors =>
            {
                this.logger.LogError("DisableTwoFactor failed: {Errors}", errors);
                this.State.SetValue(ProfileState.Error);
                return false;
            });
    }
}
