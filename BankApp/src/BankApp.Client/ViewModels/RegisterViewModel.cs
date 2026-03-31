using BankApp.Client.Utilities;
using BankApp.Core.Enums;
using BankApp.Core.DTOs.Auth;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace BankApp.Client.ViewModels
{
    public class RegisterViewModel 
    {
        public ObservableState<RegisterState> State { get; private set; }

        private readonly ApiClient _apiClient;

        public RegisterViewModel(ApiClient apiClient)
        {
            State = new ObservableState<RegisterState>(RegisterState.Idle);
            _apiClient = apiClient;
        }

        public async void Register(string email, string password, string confirmPassword, string fullName)
        {
            // Client-side validation
            RegisterState? validationError = ValidateLocally(email, password, confirmPassword, fullName);
            if (validationError != null)
            {
                State.SetValue(validationError.Value);
                return;
            }

            State.SetValue(RegisterState.Loading);

            try
            {
                RegisterRequest? request = new RegisterRequest
                {
                    Email = email,
                    Password = password,
                    FullName = fullName
                };

                RegisterResponse? response = await _apiClient.PostAsync<RegisterRequest, RegisterResponse>(
                    "/api/auth/register", request);

                if (response == null)
                {
                    State.SetValue(RegisterState.Error);
                    return;
                }

                if (!response.Success)
                {
                    HandleRegisterError(response);
                    return;
                }

                State.SetValue(RegisterState.Success);
            }
            catch (Exception)
            {
                State.SetValue(RegisterState.Error);
            }
        }

        public async void OAuthRegister(string email, string provider)
        {
            State.SetValue(RegisterState.Loading);

            try
            {
                if (provider.ToLower() == "google")
                {
                    var options = new Duende.IdentityModel.OidcClient.OidcClientOptions
                    {
                        Authority = "https://accounts.google.com",
                        ClientId = OAuthSecretsTemplate.ClientId,
                        ClientSecret = OAuthSecretsTemplate.ClientSecret,
                        Scope = "openid email profile",
                        RedirectUri = "http://127.0.0.1:7890/",
                        Browser = new BankApp.Client.Utilities.SystemBrowser(7890)
                    };
                    options.Policy.Discovery.ValidateEndpoints = false;

                    var oidcClient = new Duende.IdentityModel.OidcClient.OidcClient(options);
                    var loginResult = await oidcClient.LoginAsync(new Duende.IdentityModel.OidcClient.LoginRequest());

                    if (loginResult.IsError)
                    {
                        State.SetValue(RegisterState.Error);
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
                        State.SetValue(RegisterState.Error);
                        return;
                    }

                    _apiClient.SetToken(response.Token!);
                    _apiClient.SetCurrentUserId(response.UserId!.Value);

                    State.SetValue(RegisterState.AutoLoggedIn);
                }
            }
            catch (Exception)
            {
                State.SetValue(RegisterState.Error);
            }
        }

        private RegisterState? ValidateLocally(string email, string password, string confirmPassword, string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) { return RegisterState.Error; }

            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@")) { return RegisterState.InvalidEmail; }

            if (string.IsNullOrWhiteSpace(password) || password.Length < 8
                || !password.Any(char.IsUpper)
                || !password.Any(char.IsLower)
                || !password.Any(char.IsDigit))
                { return RegisterState.WeakPassword; }

            if (password != confirmPassword) { return RegisterState.PasswordMismatch; }
            return null;
        }

        private void HandleRegisterError(RegisterResponse response)
        {
            if (response.Error != null && response.Error.Contains("already registered"))
                State.SetValue(RegisterState.EmailAlreadyExists);
            else if (response.Error != null && response.Error.Contains("email"))
                State.SetValue(RegisterState.InvalidEmail);
            else if (response.Error != null && response.Error.Contains("Password"))
                State.SetValue(RegisterState.WeakPassword);
            else
                State.SetValue(RegisterState.Error);
        }

        public void Dispose()
        {
            // Clean up observers if needed
        }
    }
}


