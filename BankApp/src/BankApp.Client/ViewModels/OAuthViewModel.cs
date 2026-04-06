// <copyright file="OAuthViewModel.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BankApp.Client.Utilities;
using BankApp.Core.Entities;
using BankApp.Client.Enums;
using Microsoft.Extensions.Logging;

namespace BankApp.Client.ViewModels;

/// <summary>
/// Handles OAuth provider linking and unlinking operations.
/// </summary>
public class OAuthViewModel
{
    private readonly ApiClient apiClient;
    private readonly ILogger<OAuthViewModel> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="OAuthViewModel"/> class.
    /// </summary>
    /// <param name="apiClient">The API client used for OAuth operations.</param>
    /// <param name="logger">Logger for OAuth operation errors.</param>
    public OAuthViewModel(ApiClient apiClient, ILogger<OAuthViewModel> logger)
    {
        this.apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.State = new ObservableState<ProfileState>(ProfileState.Idle);
        this.OAuthLinks = new List<OAuthLink>();
    }

    /// <summary>
    /// Gets the current OAuth workflow state.
    /// </summary>
    public ObservableState<ProfileState> State { get; }

    /// <summary>
    /// Gets the linked OAuth accounts for the current user.
    /// </summary>
    public List<OAuthLink> OAuthLinks { get; private set; }

    /// <summary>
    /// Loads the OAuth links for the current user from the server.
    /// </summary>
    /// <returns><see langword="true"/> if loaded successfully; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> LoadOAuthLinks()
    {
        var oauthResult = await this.apiClient.GetAsync<List<OAuthLink>>("api/profile/oauthlinks");
        if (oauthResult.IsError)
        {
            this.logger.LogError("LoadOAuthLinks: request failed: {Errors}", oauthResult.Errors);
            return false;
        }

        this.OAuthLinks = oauthResult.Value;
        return true;
    }

    /// <summary>
    /// Links a new OAuth provider to the current account.
    /// </summary>
    /// <param name="provider">The provider to link.</param>
    /// <returns><see langword="true"/> if the provider was linked; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> LinkOAuth(string provider)
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            return false;
        }

        bool alreadyLinked = this.OAuthLinks.Exists(o =>
            string.Equals(o.Provider, provider, StringComparison.OrdinalIgnoreCase));
        if (alreadyLinked)
        {
            return false;
        }

        this.State.SetValue(ProfileState.Loading);

        var request = new { Provider = provider.Trim() };
        var result = await this.apiClient.PostAsync<object, bool>("api/profile/oauth/link", request);

        return result.Match(
            linked =>
            {
                if (!linked)
                {
                    this.State.SetValue(ProfileState.Error);
                    return false;
                }

                this.State.SetValue(ProfileState.UpdateSuccess);
                return true;
            },
            errors =>
            {
                this.logger.LogError("LinkOAuth failed: {Errors}", errors);
                this.State.SetValue(ProfileState.Error);
                return false;
            });
    }

    /// <summary>
    /// Removes a linked OAuth provider from the local profile state.
    /// </summary>
    /// <param name="provider">The provider to remove.</param>
    /// <returns><see langword="true"/> if the provider was removed; otherwise, <see langword="false"/>.</returns>
    public Task<bool> UnlinkOAuth(string provider)
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            return Task.FromResult(false);
        }

        OAuthLink? existing = this.OAuthLinks.Find(o =>
            string.Equals(o.Provider, provider, StringComparison.OrdinalIgnoreCase));
        if (existing == null)
        {
            return Task.FromResult(false);
        }

        this.OAuthLinks.Remove(existing);
        this.State.SetValue(ProfileState.UpdateSuccess);
        return Task.FromResult(true);
    }
}
