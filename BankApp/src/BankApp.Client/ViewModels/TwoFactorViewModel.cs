// <copyright file="TwoFactorViewModel.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;
using BankApp.Client.Utilities;
using BankApp.Core.DTOs.Auth;
using BankApp.Core.Enums;

namespace BankApp.Client.ViewModels
{
    /// <summary>
    /// Coordinates OTP verification and resend operations for the two-factor authentication flow.
    /// </summary>
    public class TwoFactorViewModel
    {
        private readonly ApiClient apiClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="TwoFactorViewModel"/> class.
        /// </summary>
        /// <param name="apiClient">The API client used for authentication requests.</param>
        public TwoFactorViewModel(ApiClient apiClient)
        {
            this.apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            this.State = new ObservableState<TwoFactorState>(TwoFactorState.Idle);
        }

        /// <summary>
        /// Gets the current state of the two-factor authentication flow.
        /// </summary>
        public ObservableState<TwoFactorState> State { get; }

        /// <summary>
        /// Verifies the OTP code entered by the user.
        /// </summary>
        /// <param name="otp">The one-time password to validate.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task VerifyOtp(string otp)
        {
            if (string.IsNullOrWhiteSpace(otp))
            {
                this.State.SetValue(TwoFactorState.InvalidOTP);
                return;
            }

            this.State.SetValue(TwoFactorState.Verifying);

            try
            {
                int? userId = this.apiClient.CurrentUserId;
                if (userId == null)
                {
                    this.State.SetValue(TwoFactorState.InvalidOTP);
                    return;
                }

                var request = new VerifyOTPRequest
                {
                    UserId = userId.Value,
                    OTPCode = otp,
                };

                LoginResponse? response = await this.apiClient.PostAsync<VerifyOTPRequest, LoginResponse>("/api/auth/verify-otp", request);
                if (response != null && response.Success)
                {
                    this.apiClient.SetToken(response.Token!);
                    this.State.SetValue(TwoFactorState.Success);
                    return;
                }

                this.State.SetValue(TwoFactorState.InvalidOTP);
            }
            catch (Exception)
            {
                this.State.SetValue(TwoFactorState.InvalidOTP);
            }
        }

        /// <summary>
        /// Requests a new OTP for the current user.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ResendOtp()
        {
            this.State.SetValue(TwoFactorState.Idle);

            try
            {
                int? userId = this.apiClient.CurrentUserId;
                if (userId == null)
                {
                    return;
                }

                await this.apiClient.PostAsync<object?, object>($"/api/auth/resend-otp?userId={userId.Value}", null);
            }
            catch (Exception)
            {
            }
        }
    }
}
