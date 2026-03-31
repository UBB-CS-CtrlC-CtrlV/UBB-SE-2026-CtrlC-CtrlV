using System;
using System.Threading.Tasks;
using BankApp.Client.Utilities;
using BankApp.Core.DTOs.Auth;
using BankApp.Core.Enums;

namespace BankApp.Client.ViewModels
{
    public class ApiResponse
    {
        public string? message { get; set; }
        public string? error { get; set; }
    }

    public class ForgotPasswordViewModel 
    {
        private readonly ApiClient _apiClient;
        public ObservableState<ForgotPasswordState> State { get; private set; }

        public ForgotPasswordViewModel(ApiClient apiClient)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            State = new ObservableState<ForgotPasswordState>(ForgotPasswordState.Idle);
        }

        public async Task ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                State.SetValue(ForgotPasswordState.Error);
                return;
            }

            try
            {
                var request = new ForgotPasswordRequest { Email = email };
                var response = await _apiClient.PostAsync<ForgotPasswordRequest, ApiResponse>("/api/auth/forgot-password", request);
                if (response != null && response.error == null)
                {
                    State.SetValue(ForgotPasswordState.EmailSent);
                }
                else
                {
                    State.SetValue(ForgotPasswordState.Error);
                }
            }
            catch (Exception)
            {
                State.SetValue(ForgotPasswordState.Error);
            }
        }

        public async Task ResetPassword(string email, string newPassword, string code)
        {
            if (string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(code))
            {
                State.SetValue(ForgotPasswordState.Error);
                return;
            }

            try
            {
                var request = new ResetPasswordRequest
                {
                    Token = code,
                    NewPassword = newPassword
                };
                var response = await _apiClient.PostAsync<ResetPasswordRequest, ApiResponse>("/api/auth/reset-password", request);
                if (response != null && response.error == null)
                {
                    State.SetValue(ForgotPasswordState.PasswordResetSuccess);
                }
                else
                {
                    if (response?.error != null && response.error.Contains("expired", StringComparison.OrdinalIgnoreCase))
                    {
                        State.SetValue(ForgotPasswordState.TokenExpired);
                    }
                    else
                    {
                        State.SetValue(ForgotPasswordState.Error);
                    }
                }
            }
            catch (Exception)
            {
                State.SetValue(ForgotPasswordState.Error);
            }
        }

        public async Task VerifyToken(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                State.SetValue(ForgotPasswordState.Error);
                return;
            }

            try
            {
                var response = await _apiClient.PostAsync<object, ApiResponse>("/api/auth/verify-reset-token", new { Token = code });

                if (response != null && response.error == null)
                {
                    State.SetValue(ForgotPasswordState.TokenValid);
                }
                else
                {
                    State.SetValue(ForgotPasswordState.TokenExpired);
                }
            }
            catch (Exception)
            {
                State.SetValue(ForgotPasswordState.Error);
            }
        }


        public void Dispose()
        {
        }
    }
}


