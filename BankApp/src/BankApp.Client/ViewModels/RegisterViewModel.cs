// <copyright file="RegisterViewModel.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System;
using System.Linq;
using System.Threading.Tasks;
using BankApp.Client.Utilities;
using BankApp.Core.DTOs.Auth;
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
        RegisterState? validationError = this.ValidateLocally(email, password, confirmPassword, fullName);
        if (validationError != null)
        {
            this.State.SetValue(validationError.Value);
            return;
        }

        this.State.SetValue(RegisterState.Loading);

        var request = new RegisterRequest
        {
            Email = email,
            Password = password,
            FullName = fullName,
        };

        var result = await this.apiClient.PostAsync<RegisterRequest, RegisterResponse>("/api/auth/register", request);

        result.Switch(
            response =>
            {
                if (!response.Success)
                {
                    this.HandleRegisterError(response);
                    return;
                }

                this.State.SetValue(RegisterState.Success);
            },
            errors =>
            {
                if (errors[0].Type == ErrorType.Validation)
                {
                    if (errors[0].Description.Contains("email", StringComparison.OrdinalIgnoreCase))
                    {
                        this.State.SetValue(RegisterState.InvalidEmail);
                    }
                    else
                    {
                        this.State.SetValue(RegisterState.WeakPassword);
                    }
                }
                else
                {
                    this.logger.LogError("Register failed: {Errors}", errors);
                    this.State.SetValue(RegisterState.Error);
                }
            });
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

            var authority = this.configuration["OAuth:Google:Authority"]
                ?? throw new InvalidOperationException("OAuth:Google:Authority is missing from configuration.");
            var clientId = this.configuration["OAuth:Google:ClientId"]
                ?? throw new InvalidOperationException("OAuth:Google:ClientId is missing from configuration.");
            var clientSecret = this.configuration["OAuth:Google:ClientSecret"]
                ?? throw new InvalidOperationException("OAuth:Google:ClientSecret is missing from configuration.");
            var redirectUri = this.configuration["OAuth:Google:RedirectUri"]
                ?? throw new InvalidOperationException("OAuth:Google:RedirectUri is missing from configuration.");

            var options = new Duende.IdentityModel.OidcClient.OidcClientOptions
            {
                Authority = authority,
                ClientId = clientId,
                ClientSecret = clientSecret,
                Scope = "openid email profile",
                RedirectUri = redirectUri,
                Browser = new SystemBrowser(new Uri(redirectUri).Port),
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

            var result = await this.apiClient.PostAsync<OAuthLoginRequest, LoginResponse>("/api/auth/oauth-login", apiRequest);

            result.Switch(
                response =>
                {
                    if (!response.Success)
                    {
                        this.State.SetValue(RegisterState.Error);
                        return;
                    }

                    this.apiClient.SetToken(response.Token!);
                    this.apiClient.CurrentUserId = response.UserId!.Value;
                    this.State.SetValue(RegisterState.AutoLoggedIn);
                },
                errors =>
                {
                    this.logger.LogError("OAuthRegister failed: {Errors}", errors);
                    this.State.SetValue(RegisterState.Error);
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

        if (string.IsNullOrWhiteSpace(password)
            || password.Length < 8
            || !password.Any(char.IsUpper)
            || !password.Any(char.IsLower)
            || !password.Any(char.IsDigit)
            || !password.Any(c => !char.IsLetterOrDigit(c)))
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
