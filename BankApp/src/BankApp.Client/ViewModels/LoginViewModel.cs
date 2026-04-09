// <copyright file="LoginViewModel.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;
using BankApp.Client.Utilities;
using BankApp.Contracts.DTOs.Auth;
using BankApp.Client.Enums;
using Duende.IdentityModel.OidcClient;
using ErrorOr;
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
        LoginState initialState = apiClient.EnsureConfigured().Match(
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
    /// Returns <see langword="true"/> when both <paramref name="email"/> and
    /// <paramref name="password"/> are non-empty and a login attempt can be made.
    /// </summary>
    /// <param name="email">The email address entered by the user.</param>
    /// <param name="password">The password entered by the user.</param>
    /// <returns><see langword="true"/> if the inputs are sufficient to attempt login.</returns>
    public bool CanLogin(string email, string password) =>
        !string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password);

    /// <summary>
    /// Attempts to sign in with the provided email address and password.
    /// </summary>
    /// <param name="email">The email address entered by the user.</param>
    /// <param name="password">The password entered by the user.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task Login(string email, string password)
    {
        this.State.SetValue(LoginState.Loading);

        BankApp.Contracts.DTOs.Auth.LoginRequest request = new BankApp.Contracts.DTOs.Auth.LoginRequest
        {
            Email = email,
            Password = password,
        };

        ErrorOr<LoginSuccessResponse> result = await this.apiClient.PostAsync<BankApp.Contracts.DTOs.Auth.LoginRequest, LoginSuccessResponse>(
            ApiEndpoints.Login,
            request);

        result.Switch(
            response =>
            {
                if (response.Requires2FA)
                {
                    this.apiClient.CurrentUserId = response.UserId;
                    this.State.SetValue(LoginState.Require2Fa);
                    return;
                }

                this.apiClient.SetToken(response.Token!);
                this.apiClient.CurrentUserId = response.UserId;
                this.State.SetValue(LoginState.Success);
            },
            errors =>
            {
                if (errors[0].Type == ErrorType.Forbidden)
                {
                    this.State.SetValue(LoginState.AccountLocked);
                }
                else if (errors[0].Type == ErrorType.Unauthorized)
                {
                    this.State.SetValue(LoginState.InvalidCredentials);
                }
                else
                {
                    this.logger.LogError("Login failed: {Errors}", errors);
                    this.State.SetValue(LoginState.Error);
                }
            });
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

            string authority = this.configuration["OAuth:Google:Authority"]
                ?? throw new InvalidOperationException("OAuth:Google:Authority is missing from configuration.");
            string clientId = this.configuration["OAuth:Google:ClientId"]
                ?? throw new InvalidOperationException("OAuth:Google:ClientId is missing from configuration.");
            string clientSecret = this.configuration["OAuth:Google:ClientSecret"]
                ?? throw new InvalidOperationException("OAuth:Google:ClientSecret is missing from configuration.");
            string redirectUri = this.configuration["OAuth:Google:RedirectUri"]
                ?? throw new InvalidOperationException("OAuth:Google:RedirectUri is missing from configuration.");

            OidcClientOptions options = new OidcClientOptions
            {
                Authority = authority,
                ClientId = clientId,
                ClientSecret = clientSecret,
                Scope = "openid email profile",
                RedirectUri = redirectUri,
                Browser = new SystemBrowser(new Uri(redirectUri).Port),
            };

            options.Policy.Discovery.ValidateEndpoints = false;

            OidcClient oidcClient = new OidcClient(options);
            LoginResult loginResult = await oidcClient.LoginAsync(new Duende.IdentityModel.OidcClient.LoginRequest());

            if (loginResult.IsError)
            {
                this.State.SetValue(LoginState.Error);
                return;
            }

            OAuthLoginRequest apiRequest = new OAuthLoginRequest
            {
                Provider = "Google",
                ProviderToken = loginResult.IdentityToken,
            };

            ErrorOr<LoginSuccessResponse> result = await this.apiClient.PostAsync<OAuthLoginRequest, LoginSuccessResponse>(
                ApiEndpoints.OAuthLogin,
                apiRequest);

            result.Switch(
                response =>
                {
                    if (response.Requires2FA)
                    {
                        this.apiClient.CurrentUserId = response.UserId;
                        this.State.SetValue(LoginState.Require2Fa);
                        return;
                    }

                    this.apiClient.SetToken(response.Token!);
                    this.apiClient.CurrentUserId = response.UserId;
                    this.State.SetValue(LoginState.Success);
                },
                errors =>
                {
                    this.logger.LogError("OAuthLogin failed: {Errors}", errors);
                    this.State.SetValue(LoginState.Error);
                });
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "OAuthLogin OIDC flow failed.");
            this.State.SetValue(LoginState.Error);
        }
    }
}
