// <copyright file="RegisterViewModel.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System;
using System.Linq;
using System.Threading.Tasks;
using BankApp.Client.Utilities;
using BankApp.Core.DTOs.Auth;
using BankApp.Core.Enums;

namespace BankApp.Client.ViewModels;

/// <summary>
/// Coordinates registration requests for the register view.
/// </summary>
public class RegisterViewModel
{
    private readonly ApiClient apiClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterViewModel"/> class.
    /// </summary>
    /// <param name="apiClient">The API client used for registration requests.</param>
    public RegisterViewModel(ApiClient apiClient)
    {
        this.apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        this.State = new ObservableState<RegisterState>(RegisterState.Idle);
    }

    /// <summary>
    /// Gets the current state of the registration flow.
    /// </summary>
    public ObservableState<RegisterState> State { get; }

    /// <summary>
    /// Registers a new account using email and password credentials.
    /// </summary>
    /// <param name="email">The email address entered by the user.</param>
    /// <param name="password">The password entered by the user.</param>
    /// <param name="confirmPassword">The confirmation password entered by the user.</param>
    /// <param name="fullName">The full name entered by the user.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task Register(string email, string password, string confirmPassword, string fullName)
    {
        RegisterState? validationError = this.ValidateLocally(email, password, confirmPassword, fullName);
        if (validationError != null)
        {
            this.State.SetValue(validationError.Value);
            return;
        }

        this.State.SetValue(RegisterState.Loading);

        try
        {
            var request = new RegisterRequest
            {
                Email = email,
                Password = password,
                FullName = fullName,
            };

            RegisterResponse? response = await this.apiClient.PostAsync<RegisterRequest, RegisterResponse>("/api/auth/register", request);
            if (response == null)
            {
                this.State.SetValue(RegisterState.Error);
                return;
            }

            if (!response.Success)
            {
                this.HandleRegisterError(response);
                return;
            }

            this.State.SetValue(RegisterState.Success);
        }
        catch (Exception)
        {
            this.State.SetValue(RegisterState.Error);
        }
    }

    /// <summary>
    /// Registers or signs in a user through the specified OAuth provider.
    /// </summary>
    /// <param name="provider">The OAuth provider to use.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task OAuthRegister(string provider)
    {
        this.State.SetValue(RegisterState.Loading);

        try
        {
            if (!provider.Equals("google", StringComparison.OrdinalIgnoreCase))
            {
                this.State.SetValue(RegisterState.Error);
                return;
            }

            var options = new Duende.IdentityModel.OidcClient.OidcClientOptions
            {
                Authority = "https://accounts.google.com",
                ClientId = OAuthSecretsTemplate.ClientId,
                ClientSecret = OAuthSecretsTemplate.ClientSecret,
                Scope = "openid email profile",
                RedirectUri = "http://127.0.0.1:7890/",
                Browser = new SystemBrowser(7890),
            };
            options.Policy.Discovery.ValidateEndpoints = false;

            var oidcClient = new Duende.IdentityModel.OidcClient.OidcClient(options);
            var loginResult = await oidcClient.LoginAsync(new Duende.IdentityModel.OidcClient.LoginRequest());
            if (loginResult.IsError)
            {
                this.State.SetValue(RegisterState.Error);
                return;
            }

            var apiRequest = new OAuthLoginRequest
            {
                Provider = "Google",
                ProviderToken = loginResult.IdentityToken,
            };

            LoginResponse? response = await this.apiClient.PostAsync<OAuthLoginRequest, LoginResponse>("/api/auth/oauth-login", apiRequest);
            if (response == null || !response.Success)
            {
                this.State.SetValue(RegisterState.Error);
                return;
            }

            this.apiClient.SetToken(response.Token!);
            this.apiClient.CurrentUserId = response.UserId!.Value;
            this.State.SetValue(RegisterState.AutoLoggedIn);
        }
        catch (Exception)
        {
            this.State.SetValue(RegisterState.Error);
        }
    }

    private RegisterState? ValidateLocally(string email, string password, string confirmPassword, string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return RegisterState.Error;
        }

        if (string.IsNullOrWhiteSpace(email) || !email.Contains("@", StringComparison.Ordinal))
        {
            return RegisterState.InvalidEmail;
        }

        if (string.IsNullOrWhiteSpace(password)
            || password.Length < 8
            || !password.Any(char.IsUpper)
            || !password.Any(char.IsLower)
            || !password.Any(char.IsDigit))
        {
            return RegisterState.WeakPassword;
        }

        if (password != confirmPassword)
        {
            return RegisterState.PasswordMismatch;
        }

        return null;
    }

    private void HandleRegisterError(RegisterResponse response)
    {
        if (response.Error != null && response.Error.Contains("already registered", StringComparison.OrdinalIgnoreCase))
        {
            this.State.SetValue(RegisterState.EmailAlreadyExists);
        }
        else if (response.Error != null && response.Error.Contains("email", StringComparison.OrdinalIgnoreCase))
        {
            this.State.SetValue(RegisterState.InvalidEmail);
        }
        else if (response.Error != null && response.Error.Contains("password", StringComparison.OrdinalIgnoreCase))
        {
            this.State.SetValue(RegisterState.WeakPassword);
        }
        else
        {
            this.State.SetValue(RegisterState.Error);
        }
    }
}
