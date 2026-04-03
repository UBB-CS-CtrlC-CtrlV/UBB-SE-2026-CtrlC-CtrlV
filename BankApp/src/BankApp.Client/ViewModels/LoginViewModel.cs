// <copyright file="LoginViewModel.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;
using BankApp.Client.Utilities;
using BankApp.Core.DTOs.Auth;
using BankApp.Core.Enums;
using Duende.IdentityModel.OidcClient;

namespace BankApp.Client.ViewModels;

/// <summary>
/// Coordinates credential-based and OAuth login requests for the login view.
/// </summary>
public class LoginViewModel
{
    private readonly ApiClient apiClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginViewModel"/> class.
    /// </summary>
    /// <param name="apiClient">The API client used for authentication requests.</param>
    public LoginViewModel(ApiClient apiClient)
    {
        this.State = new ObservableState<LoginState>(LoginState.Idle);
        this.apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
    }

    /// <summary>
    /// Gets the current login flow state.
    /// </summary>
    public ObservableState<LoginState> State { get; }

    /// <summary>
    /// Attempts to sign in with the provided email address and password.
    /// </summary>
    /// <param name="email">The email address entered by the user.</param>
    /// <param name="password">The password entered by the user.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task Login(string email, string password)
    {
        this.State.SetValue(LoginState.Loading);

        try
        {
            var request = new Core.DTOs.Auth.LoginRequest
            {
                Email = email,
                Password = password,
            };

            var response = await this.apiClient.PostAsync<Core.DTOs.Auth.LoginRequest, LoginResponse>(
                "/api/auth/login",
                request);

            if (response == null)
            {
                this.State.SetValue(LoginState.Error);
                return;
            }

            if (!response.Success)
            {
                this.HandleLoginError(response);
                return;
            }

            if (response.Requires2FA)
            {
                this.apiClient.CurrentUserId = response.UserId!.Value;

                this.State.SetValue(LoginState.Require2FA);
                return;
            }

            // Login successful
            // Store the token and userId for future requests
            this.apiClient.SetToken(response.Token!);
            this.apiClient.CurrentUserId = response.UserId!.Value;
            this.State.SetValue(LoginState.Success);
        }
        catch (Exception)
        {
            this.State.SetValue(LoginState.Error);
        }
    }

    /// <summary>
    /// Attempts to sign in with the specified OAuth provider.
    /// </summary>
    /// <param name="provider">The OAuth provider to authenticate against.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    public async Task OAuthLogin(string provider)
    {
        this.State.SetValue(LoginState.Loading);

        try
        {
            if (!provider.Equals("google", StringComparison.OrdinalIgnoreCase))
            {
                this.State.SetValue(LoginState.Error);
                return;
            }

            var options = new OidcClientOptions
            {
                Authority = "https://accounts.google.com",
                ClientId = OAuthSecretsTemplate.ClientId,
                ClientSecret = OAuthSecretsTemplate.ClientSecret,
                Scope = "openid email profile",
                RedirectUri = "http://127.0.0.1:7890/",
                Browser = new SystemBrowser(7890),
            };

            options.Policy.Discovery.ValidateEndpoints = false;

            var oidcClient = new OidcClient(options);

            var loginResult = await oidcClient.LoginAsync(new Duende.IdentityModel.OidcClient.LoginRequest());

            if (loginResult.IsError)
            {
                this.State.SetValue(LoginState.Error);
                return;
            }

            var apiRequest = new OAuthLoginRequest
            {
                Provider = "Google",
                ProviderToken = loginResult.IdentityToken,
            };

            var response = await this.apiClient.PostAsync<OAuthLoginRequest, LoginResponse>(
                "/api/auth/oauth-login",
                apiRequest);

            if (response is not { Success: true })
            {
                this.State.SetValue(LoginState.Error);
                return;
            }

            if (response.Requires2FA)
            {
                this.apiClient.CurrentUserId = response.UserId!.Value;
                this.State.SetValue(LoginState.Require2FA);
                return;
            }

            this.apiClient.SetToken(response.Token!);
            this.apiClient.CurrentUserId = response.UserId!.Value;
            this.State.SetValue(LoginState.Success);
        }
        catch (Exception)
        {
            this.State.SetValue(LoginState.Error);
        }
    }

    private void HandleLoginError(LoginResponse response)
    {
        if (response.Error != null && response.Error.Contains("locked", StringComparison.OrdinalIgnoreCase))
        {
            this.State.SetValue(LoginState.AccountLocked);
        }
        else
        {
            this.State.SetValue(LoginState.InvalidCredentials);
        }
    }
}
