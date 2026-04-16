// <copyright file="RegisterViewModel.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;
using BankApp.Client.Utilities;
using BankApp.Contracts.DTOs.Auth;
using BankApp.Client.Enums;
using ErrorOr;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BankApp.Client.ViewModels;

/// <summary>
/// Coordinates registration requests for the register view.
/// </summary>
public class RegisterViewModel
{
    private readonly ApiClient apiClient;
    private readonly IConfiguration configuration;
    private readonly ILogger<RegisterViewModel> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterViewModel"/> class.
    /// </summary>
    /// <param name="apiClient">The API client used for registration requests.</param>
    /// <param name="configuration">
    /// The application configuration. Reads <c>OAuth:Google:Authority</c>,
    /// <c>OAuth:Google:ClientId</c>, <c>OAuth:Google:ClientSecret</c>, and
    /// <c>OAuth:Google:RedirectUri</c> when performing an OAuth registration.
    /// </param>
    /// <param name="logger">Logger for registration flow errors.</param>
    public RegisterViewModel(ApiClient apiClient, IConfiguration configuration, ILogger<RegisterViewModel> logger)
    {
        this.apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
        email = email?.Trim() ?? string.Empty;
        fullName = fullName?.Trim() ?? string.Empty;

        RegisterState? validationError = this.ValidateLocally(email, password, confirmPassword, fullName);
        if (validationError != null)
        {
            this.State.SetValue(validationError.Value);
            return;
        }

        this.State.SetValue(RegisterState.Loading);

        RegisterRequest request = new RegisterRequest
        {
            Email = email,
            Password = password,
            FullName = fullName,
        };

        ErrorOr<Success> result = await this.apiClient.PostAsync<RegisterRequest>(ApiEndpoints.Register, request);

        result.Switch(
            _ => { this.State.SetValue(RegisterState.Success); },
            errors =>
            {
                Error error = errors[0];
                if (error.Type == ErrorType.Conflict)
                {
                    this.State.SetValue(RegisterState.EmailAlreadyExists);
                }
                else if (error.Code == "invalid_email")
                {
                    this.State.SetValue(RegisterState.InvalidEmail);
                }
                else if (error.Code == "weak_password")
                {
                    this.State.SetValue(RegisterState.WeakPassword);
                }
                else
                {
                    this.logger.LogError("Register failed: {Errors}", errors);
                    this.State.SetValue(RegisterState.Error);
                }
            });
    }

   /// <summary>
/// Registers a user through the specified OAuth provider.
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

        string authority = this.configuration["OAuth:Google:Authority"]
            ?? throw new InvalidOperationException("OAuth:Google:Authority is missing from configuration.");
        string clientId = this.configuration["OAuth:Google:ClientId"]
            ?? throw new InvalidOperationException("OAuth:Google:ClientId is missing from configuration.");
        string clientSecret = this.configuration["OAuth:Google:ClientSecret"]
            ?? throw new InvalidOperationException("OAuth:Google:ClientSecret is missing from configuration.");
        string redirectUri = this.configuration["OAuth:Google:RedirectUri"]
            ?? throw new InvalidOperationException("OAuth:Google:RedirectUri is missing from configuration.");

        Duende.IdentityModel.OidcClient.OidcClientOptions options = new Duende.IdentityModel.OidcClient.OidcClientOptions
        {
            Authority = authority,
            ClientId = clientId,
            ClientSecret = clientSecret,
            Scope = "openid email profile",
            RedirectUri = redirectUri,
            Browser = new SystemBrowser(new Uri(redirectUri).Port),
        };
        options.Policy.Discovery.ValidateEndpoints = false;

        Duende.IdentityModel.OidcClient.OidcClient oidcClient = new Duende.IdentityModel.OidcClient.OidcClient(options);
        Duende.IdentityModel.OidcClient.LoginResult loginResult = await oidcClient.LoginAsync(new Duende.IdentityModel.OidcClient.LoginRequest());
        if (loginResult.IsError)
        {
            this.State.SetValue(RegisterState.Error);
            return;
        }

        OAuthRegisterRequest apiRequest = new OAuthRegisterRequest
        {
            Provider = "Google",
            ProviderToken = loginResult.IdentityToken,
            FullName = loginResult.User?.FindFirst("name")?.Value ?? string.Empty,
            Email = loginResult.User?.FindFirst("email")?.Value ?? string.Empty,
        };

        ErrorOr<Success> result = await this.apiClient.PostAsync<OAuthRegisterRequest>(ApiEndpoints.OAuthRegister, apiRequest);

        result.Switch(
            _ =>
            {
                this.State.SetValue(RegisterState.Success);
            },
            errors =>
            {
                Error error = errors[0];
                if (error.Type == ErrorType.Conflict)
                {
                    this.State.SetValue(RegisterState.EmailAlreadyExists);
                }
                else
                {
                    this.logger.LogError("OAuthRegister failed: {Errors}", errors);
                    this.State.SetValue(RegisterState.Error);
                }
            });
    }
    catch (Exception ex)
    {
        this.logger.LogError(ex, "OAuthRegister OIDC flow failed.");
        this.State.SetValue(RegisterState.Error);
    }
}

    private RegisterState? ValidateLocally(string email, string password, string confirmPassword, string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName)
            || string.IsNullOrWhiteSpace(email)
            || string.IsNullOrWhiteSpace(password)
            || string.IsNullOrWhiteSpace(confirmPassword))
        {
            return RegisterState.Error;
        }

        if (string.IsNullOrWhiteSpace(email) || !email.Contains("@", StringComparison.Ordinal))
        {
            return RegisterState.InvalidEmail;
        }

        if (!PasswordValidator.IsStrong(password))
        {
            return RegisterState.WeakPassword;
        }

        if (password != confirmPassword)
        {
            return RegisterState.PasswordMismatch;
        }

        return null;
    }
}
