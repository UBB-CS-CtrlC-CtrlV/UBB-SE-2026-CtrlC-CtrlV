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
    public class TwoFactorViewModel 
    {
        private readonly ApiClient _apiClient;
        public ObservableState<TwoFactorState> State { get; private set; }

        public TwoFactorViewModel(ApiClient apiClient)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            State = new ObservableState<TwoFactorState>(TwoFactorState.Idle);
        }

        public async Task VerifyOTP(string otp)
        {
            if (string.IsNullOrWhiteSpace(otp))
            {
                State.SetValue(TwoFactorState.InvalidOTP);
                return;
            }

            State.SetValue(TwoFactorState.Verifying);

            try
            {
                int? userId = _apiClient.GetCurrentUserId();
                if (userId == null)
                {
                    State.SetValue(TwoFactorState.InvalidOTP);
                    return;
                }

                var request = new VerifyOTPRequest
                {
                    UserId = userId.Value,
                    OTPCode = otp
                };

                var response = await _apiClient.PostAsync<VerifyOTPRequest, LoginResponse>("/api/auth/verify-otp", request);

                if (response != null && response.Success)
                {
                    _apiClient.SetToken(response.Token!);
                    State.SetValue(TwoFactorState.Success);
                }
                else
                {
                    State.SetValue(TwoFactorState.InvalidOTP);
                }
            }
            catch (Exception)
            {
                State.SetValue(TwoFactorState.InvalidOTP);
            }
        }

        public async Task ResendOTP()
        {
            State.SetValue(TwoFactorState.Idle);
            try
            {
                int? userId = _apiClient.GetCurrentUserId();
                if (userId == null) return;
                await _apiClient.PostAsync<object, object>($"/api/auth/resend-otp?userId={userId.Value}", null);
            }
            catch (Exception)
            {
                ;
            }
        }

        public void Dispose()
        {
        }
    }
}


