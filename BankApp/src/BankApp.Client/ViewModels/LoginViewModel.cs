// <copyright file="LoginViewModel.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;
using BankApp.Client.Utilities;
using BankApp.Core.DTOs.Auth;
using BankApp.Core.Enums;
using Duende.IdentityModel.OidcClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BankApp.Client.ViewModels;

/// <summary>
/// Coordinates credential-based and OAuth login requests for the login view.
/// </summary>
public class LoginViewModel
{
    private readonly ApiClient apiClient;
    private readonly IConfiguration configuration;
    private readonly ILogger<LoginViewModel> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginViewModel"/> class.
    /// </summary>
    /// <param name="apiClient">The API client used for authentication requests.</param>
    /// <param name="configuration">
    /// The application configuration. Reads <c>OAuth:Google:Authority</c>,
    /// <c>OAuth:Google:ClientId</c>, <c>OAuth:Google:ClientSecret</c>, and
    /// <c>OAuth:Google:RedirectUri</c> when performing an OAuth login.
    /// </param>
    /// <param name="logger">Logger for login flow diagnostics and errors.</param>
    public LoginViewModel(ApiClient apiClient, IConfiguration configuration, ILogger<LoginViewModel> logger)
    {
        this.apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Determine initial state from configuration. If ApiClient is misconfigured the
        // view starts in ServerNotConfigured so the login form is disabled immediately.
        // The view reads State.Value after subscribing to apply this initial state.
        var initialState = apiClient.EnsureConfigured().Match(
            _ => LoginState.Idle,
            errors =>
            {
                this.logger.LogCritical("ApiClient is not configured — login is unavailable. Errors: {Errors}", errors);
                return LoginState.ServerNotConfigured;
            });

        this.State = new ObservableState<LoginState>(initialState);
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

                this.State.SetValue(LoginState.Require2Fa);
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

            var authority = this.configuration["OAuth:Google:Authority"]
                ?? throw new InvalidOperationException("OAuth:Google:Authority is missing from configuration.");
            var clientId = this.configuration["OAuth:Google:ClientId"]
                ?? throw new InvalidOperationException("OAuth:Google:ClientId is missing from configuration.");
            var clientSecret = this.configuration["OAuth:Google:ClientSecret"]
                ?? throw new InvalidOperationException("OAuth:Google:ClientSecret is missing from configuration.");
            var redirectUri = this.configuration["OAuth:Google:RedirectUri"]
                ?? throw new InvalidOperationException("OAuth:Google:RedirectUri is missing from configuration.");

            var options = new OidcClientOptions
            {
                Authority = authority,
                ClientId = clientId,
                ClientSecret = clientSecret,
                Scope = "openid email profile",
                RedirectUri = redirectUri,
                Browser = new SystemBrowser(new Uri(redirectUri).Port),
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
                this.State.SetValue(LoginState.Require2Fa);
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
