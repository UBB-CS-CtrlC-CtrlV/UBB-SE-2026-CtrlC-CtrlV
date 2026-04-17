// <copyright file="PasswordRecoveryManager.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System;
using System.Linq;
using System.Threading.Tasks;
using BankApp.Application.DataTransferObjects.Auth;
using BankApp.Desktop.Enums;
using ErrorOr;

namespace BankApp.Desktop.Utilities;

/// <summary>
/// Implements <see cref="IPasswordRecoveryManager"/> by delegating network calls
/// to <see cref="IApiClient"/> and managing resend-throttling via <see cref="ISystemClock"/>.
/// </summary>
public class PasswordRecoveryManager : IPasswordRecoveryManager
{
    private const int ResendCooldownSeconds = 60;
    private const int NoSecondsRemaining = 0;

    private readonly IApiClient apiClient;
    private readonly ISystemClock clock;

    private DateTime? lastCodeRequestedAt;

    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordRecoveryManager"/> class.
    /// </summary>
    /// <param name="apiClient">The HTTP client used to reach the auth API.</param>
    /// <param name="clock">The system clock abstraction used for throttle calculations.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public PasswordRecoveryManager(IApiClient apiClient, ISystemClock clock)
    {
        this.apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    /// <inheritdoc/>
    public bool CanResendCode
    {
        get
        {
            if (this.lastCodeRequestedAt is null)
            {
                return true;
            }

            return (this.clock.UtcNow - this.lastCodeRequestedAt.Value).TotalSeconds >= ResendCooldownSeconds;
        }
    }

    /// <inheritdoc/>
    public int SecondsUntilResendAllowed
    {
        get
        {
            if (this.lastCodeRequestedAt is null)
            {
                return NoSecondsRemaining;
            }

            var elapsed = (this.clock.UtcNow - this.lastCodeRequestedAt.Value).TotalSeconds;
            var remaining = ResendCooldownSeconds - elapsed;
            return remaining > default(double) ? (int)Math.Ceiling(remaining) : NoSecondsRemaining;
        }
    }

    /// <inheritdoc/>
    public async Task<ForgotPasswordState> RequestCodeAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return ForgotPasswordState.Error;
        }

        if (!this.CanResendCode)
        {
            return ForgotPasswordState.EmailSent;
        }

        var request = new ForgotPasswordRequest { Email = email };
        var result = await this.apiClient.PostAsync<ForgotPasswordRequest, ApiResponse>(
            ApiEndpoints.ForgotPassword, request);

        return result.Match(
            response =>
            {
                if (response.Error == null)
                {
                    this.lastCodeRequestedAt = this.clock.UtcNow;
                    return ForgotPasswordState.EmailSent;
                }

                return ForgotPasswordState.Error;
            },
            _ => ForgotPasswordState.Error);
    }

    /// <inheritdoc/>
    public async Task<ForgotPasswordState> VerifyTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return ForgotPasswordState.Error;
        }

        var result = await this.apiClient.PostAsync(
            ApiEndpoints.VerifyResetToken, new { Token = token });

        return result.Match(
            _ => ForgotPasswordState.TokenValid,
            errors => this.MapError(errors.First()));
    }

    /// <inheritdoc/>
    public async Task<ForgotPasswordState> ResetPasswordAsync(string token, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(newPassword))
        {
            return ForgotPasswordState.Error;
        }

        var request = new ResetPasswordRequest
        {
            Token = token,
            NewPassword = newPassword,
        };

        var result = await this.apiClient.PostAsync(
            ApiEndpoints.ResetPassword, request);

        return result.Match(
            _ => ForgotPasswordState.PasswordResetSuccess,
            errors => this.MapError(errors.First()));
    }

    /// <inheritdoc/>
    public bool IsPasswordValid(string password)
    {
        return PasswordValidator.IsStrong(password);
    }

    private ForgotPasswordState MapError(Error error)
    {
        return error.Code switch
        {
            "token_expired" => ForgotPasswordState.TokenExpired,
            "token_already_used" => ForgotPasswordState.TokenAlreadyUsed,
            _ => ForgotPasswordState.Error,
        };
    }
}
