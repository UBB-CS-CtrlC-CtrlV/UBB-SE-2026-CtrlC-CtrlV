// <copyright file="TwoFactorViewModel.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BankApp.Client.Utilities;
using BankApp.Core.DTOs.Auth;
using BankApp.Client.Enums;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace BankApp.Client.ViewModels;

/// <summary>
/// Coordinates OTP verification and resend operations for the two-factor authentication flow.
/// Owns the resend-countdown state and exposes observable properties that the view
/// binds to directly via <c>{x:Bind}</c>, keeping all business decisions out of the
/// code-behind.
/// </summary>
public class TwoFactorViewModel : INotifyPropertyChanged
{
    private const int ResendCooldownSeconds = 30;
    private const int OtpRequiredLength = 6;

    private readonly ApiClient apiClient;
    private readonly ICountdownTimer countdownTimer;
    private readonly ILogger<TwoFactorViewModel> logger;

    private string otpCode = string.Empty;
    private int secondsRemaining;
    private bool isLoading;
    private bool isLocked;
    private string errorMessage = string.Empty;
    private bool hasError;

    /// <summary>
    /// Initializes a new instance of the <see cref="TwoFactorViewModel"/> class.
    /// </summary>
    /// <param name="apiClient">The API client used for authentication requests.</param>
    /// <param name="countdownTimer">
    /// An injectable timer abstraction. Pass a <see cref="DispatcherCountdownTimer"/>
    /// in production or a test double in unit tests.
    /// </param>
    /// <param name="logger">Logger for OTP verification and resend errors.</param>
    public TwoFactorViewModel(ApiClient apiClient, ICountdownTimer countdownTimer, ILogger<TwoFactorViewModel> logger)
    {
        this.apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        this.countdownTimer = countdownTimer ?? throw new ArgumentNullException(nameof(countdownTimer));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.State = new ObservableState<TwoFactorState>(TwoFactorState.Idle);
        this.countdownTimer.Tick += this.OnCountdownTick;
    }

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    // ─── State ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Gets the current state of the two-factor authentication flow.
    /// </summary>
    public ObservableState<TwoFactorState> State { get; }

    // ─── Input ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Gets or sets the OTP code typed by the user.
    /// Set by the view via a <c>TextChanged</c> handler — no Two-Way binding needed.
    /// </summary>
    public string OtpCode
    {
        get => this.otpCode;
        set => this.SetField(ref this.otpCode, value);
    }

    // ─── Loading / Lock ───────────────────────────────────────────────────────

    /// <summary>
    /// Gets a value indicating whether a network operation is in progress.
    /// </summary>
    public bool IsLoading
    {
        get => this.isLoading;
        private set
        {
            if (this.SetField(ref this.isLoading, value))
            {
                this.OnPropertyChanged(nameof(this.IsInputEnabled));
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether the user's input has been locked out
    /// due to too many failed attempts.
    /// </summary>
    public bool IsLocked
    {
        get => this.isLocked;
        private set
        {
            if (this.SetField(ref this.isLocked, value))
            {
                this.OnPropertyChanged(nameof(this.IsInputEnabled));
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether the OTP input and verify button are enabled.
    /// Binds to both <c>OtpBox.IsEnabled</c> and <c>VerifyButton.IsEnabled</c>.
    /// </summary>
    public bool IsInputEnabled => !this.isLoading && !this.isLocked;

    // ─── Error ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Gets the current error message, or an empty string when there is no active error.
    /// </summary>
    public string ErrorMessage
    {
        get => this.errorMessage;
        private set => this.SetField(ref this.errorMessage, value);
    }

    /// <summary>
    /// Gets a value indicating whether an error message is currently active.
    /// Binds to <c>ErrorInfoBar.IsOpen</c>.
    /// </summary>
    public bool HasError
    {
        get => this.hasError;
        private set => this.SetField(ref this.hasError, value);
    }

    // ─── Countdown ────────────────────────────────────────────────────────────

    /// <summary>
    /// Gets the number of seconds remaining before the resend button becomes available.
    /// </summary>
    public int SecondsRemaining
    {
        get => this.secondsRemaining;
        private set
        {
            if (this.SetField(ref this.secondsRemaining, value))
            {
                this.OnPropertyChanged(nameof(this.CanResend));
                this.OnPropertyChanged(nameof(this.IsCountdownVisible));
                this.OnPropertyChanged(nameof(this.CountdownDisplayText));
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether the user is allowed to request a new code.
    /// Becomes <see langword="true"/> once the countdown reaches zero.
    /// </summary>
    public bool CanResend => this.secondsRemaining <= 0;

    /// <summary>
    /// Gets a value indicating whether the countdown label should be displayed.
    /// </summary>
    public bool IsCountdownVisible => this.secondsRemaining > 0;

    /// <summary>
    /// Gets the formatted countdown string shown next to the resend button,
    /// e.g. <c>"Available in 27s"</c>.
    /// </summary>
    public string CountdownDisplayText => $"Available in {this.secondsRemaining}s";

    // ─── Commands ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Validates and submits the current <see cref="OtpCode"/> to the API.
    /// Length validation (6 digits) is enforced here — not in the view.
    /// Sets <see cref="TwoFactorState.Success"/> on a valid code, or
    /// <see cref="TwoFactorState.InvalidOTP"/> when the code is rejected or the
    /// session has expired.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task VerifyOtp()
    {
        this.ClearError();

        if (string.IsNullOrWhiteSpace(this.OtpCode) || this.OtpCode.Length != OtpRequiredLength)
        {
            this.SetError(UserMessages.TwoFactor.InvalidCodeFormat);
            return;
        }

        this.IsLoading = true;
        this.State.SetValue(TwoFactorState.Verifying);

        int? userId = this.apiClient.CurrentUserId;
        if (userId == null)
        {
            this.ApplyInvalidOtp();
            return;
        }

        var request = new VerifyOTPRequest
        {
            UserId = userId.Value,
            OTPCode = this.OtpCode,
        };

        var result = await this.apiClient.PostAsync<VerifyOTPRequest, LoginResponse>(
            "/api/auth/verify-otp", request);

        result.Switch(
            response =>
            {
                if (response.Success)
                {
                    this.apiClient.SetToken(response.Token!);
                    this.IsLoading = false;
                    this.State.SetValue(TwoFactorState.Success);
                    return;
                }

                this.ApplyInvalidOtp();
            },
            errors =>
            {
                if (errors[0].Type != ErrorType.Unauthorized)
                {
                    this.logger.LogError("VerifyOtp failed: {Errors}", errors);
                }

                this.ApplyInvalidOtp();
            });
    }

    /// <summary>
    /// Requests a new OTP for the current user.
    /// Does nothing if <see cref="CanResend"/> is <see langword="false"/> —
    /// the view may call this unconditionally and the guard here prevents
    /// duplicate or premature API calls.
    /// Failures are logged but do not change the view state; resend is best-effort.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task ResendOtp()
    {
        if (!this.CanResend)
        {
            return;
        }

        this.ClearError();
        this.SecondsRemaining = ResendCooldownSeconds;
        this.countdownTimer.Start();
        this.State.SetValue(TwoFactorState.Idle);

        int? userId = this.apiClient.CurrentUserId;
        if (userId == null)
        {
            return;
        }

        var result = await this.apiClient.PostAsync<object?, object>(
            $"/api/auth/resend-otp?userId={userId.Value}", null);

        result.Switch(
            _ => { },
            errors => this.logger.LogError("ResendOtp failed: {Errors}", errors));
    }

    // ─── Internal ─────────────────────────────────────────────────────────────
    private void OnCountdownTick(object? sender, EventArgs e)
    {
        if (this.secondsRemaining > 0)
        {
            this.SecondsRemaining--;
        }

        if (this.secondsRemaining <= 0)
        {
            this.countdownTimer.Stop();
        }
    }

    private void ApplyInvalidOtp()
    {
        this.IsLoading = false;
        this.SetError(UserMessages.TwoFactor.IncorrectCode);
        this.State.SetValue(TwoFactorState.InvalidOTP);
    }

    private void SetError(string message)
    {
        this.ErrorMessage = message;
        this.HasError = true;
    }

    private void ClearError()
    {
        this.HasError = false;
        this.ErrorMessage = string.Empty;
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    /// <summary>
    /// Sets <paramref name="field"/> to <paramref name="value"/> and raises
    /// <see cref="PropertyChanged"/> if the value actually changed.
    /// </summary>
    /// <returns><see langword="true"/> if the value changed; otherwise <see langword="false"/>.</returns>
    private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        this.OnPropertyChanged(propertyName);
        return true;
    }
}
