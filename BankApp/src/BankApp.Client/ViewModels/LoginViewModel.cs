using BankApp.Client.Utilities;
using BankApp.Core.DTOs.Auth;
using BankApp.Core.Enums;
using Duende.IdentityModel.OidcClient;
using System;
using System.Threading.Tasks;

namespace BankApp.Client.ViewModels
{
    public class LoginViewModel 
    {
        public ObservableState<LoginState> State { get; private set; }
        private readonly ApiClient _apiClient;

        public LoginViewModel(ApiClient apiClient)
        {
            State = new ObservableState<LoginState>(LoginState.Idle);
            _apiClient = apiClient;
        }

        public async void Login(string email, string password)
        {
            State.SetValue(LoginState.Loading);

            try
            {
                BankApp.Core.DTOs.Auth.LoginRequest request = new BankApp.Core.DTOs.Auth.LoginRequest
                {
                    Email = email,
                    Password = password
                };

                LoginResponse? response = await _apiClient.PostAsync<BankApp.Core.DTOs.Auth.LoginRequest, LoginResponse>(
                    "/api/auth/login", request);

                if (response == null)
                {
                    State.SetValue(LoginState.Error);
                    return;
                }

                if (!response.Success)
                {
                    HandleLoginError(response);
                    return;
                }

                if (response.Requires2FA)
                {
                    _apiClient.SetCurrentUserId(response.UserId!.Value);

                    State.SetValue(LoginState.Require2FA);
                    return;
                }

                // Login successful
                // Store the token and userId for future requests
                _apiClient.SetToken(response.Token!);
                _apiClient.SetCurrentUserId(response.UserId!.Value);
                State.SetValue(LoginState.Success);
            }
            catch (Exception)
            {
                State.SetValue(LoginState.Error);
            }
        }

        public async void OAuthLogin(string email, string provider)
        {
            State.SetValue(LoginState.Loading);

            try
            {
                if (provider.ToLower() == "google")
                {
                    var options = new OidcClientOptions
                    {
                        Authority = "https://accounts.google.com",
                        ClientId = OAuthSecretsTemplate.ClientId,
                        ClientSecret = OAuthSecretsTemplate.ClientSecret,
                        Scope = "openid email profile",
                        RedirectUri = "http://127.0.0.1:7890/",
                        Browser = new BankApp.Client.Utilities.SystemBrowser(7890)
                    };

                    options.Policy.Discovery.ValidateEndpoints = false;

                    var oidcClient = new OidcClient(options);

                    var loginResult = await oidcClient.LoginAsync(new Duende.IdentityModel.OidcClient.LoginRequest());

                    if (loginResult.IsError)
                    {
                        State.SetValue(LoginState.Error);
                        return;
                    }

                    OAuthLoginRequest apiRequest = new OAuthLoginRequest
                    {
                        Provider = "Google",
                        ProviderToken = loginResult.IdentityToken
                    };

                    LoginResponse? response = await _apiClient.PostAsync<OAuthLoginRequest, LoginResponse>(
                        "/api/auth/oauth-login", apiRequest);

                    if (response == null || !response.Success)
                    {
                        State.SetValue(LoginState.Error);
                        return;
                    }

                    if (response.Requires2FA)
                    {
                        _apiClient.SetCurrentUserId(response.UserId!.Value);
                        State.SetValue(LoginState.Require2FA);
                        return;
                    }

                    _apiClient.SetToken(response.Token!);
                    _apiClient.SetCurrentUserId(response.UserId!.Value);
                    State.SetValue(LoginState.Success);
                }
            }
            catch (Exception ex)
            {
                State.SetValue(LoginState.Error);
            }
        }

        private void HandleLoginError(LoginResponse response)
        {
            if (response.Error != null && response.Error.Contains("locked"))
            {
                State.SetValue(LoginState.AccountLocked);
            }
            else
            {
                State.SetValue(LoginState.InvalidCredentials);
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}


