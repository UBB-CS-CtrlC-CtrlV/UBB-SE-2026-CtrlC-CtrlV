// <copyright file="OAuthViewModel.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BankApp.Desktop.Enums;
using BankApp.Desktop.Utilities;
using BankApp.Application.DataTransferObjects.Profile;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace BankApp.Desktop.ViewModels;

/// <summary>
/// Handles OAuth provider linking and unlinking operations. // To Do: Change to OAuth
/// </summary>
public class OAuthViewModel
{
    private readonly IApiClient apiClient;
    private readonly ILogger<OAuthViewModel> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="OAuthViewModel"/> class.
    /// </summary>
    /// <param name="apiClient">The API client used for OAuth operations.</param> // To Do: Change to OAuth
    /// <param name="logger">Logger for OAuth operation errors.</param> // To Do: Change to OAuth
    public OAuthViewModel(IApiClient apiClient, ILogger<OAuthViewModel> logger)
    {
        this.apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.State = new ObservableState<ProfileState>(ProfileState.Idle);
        this.OAuthLinks = new List<OAuthLinkDataTransferObject>(); // To Do: Change to OAuth
    }

    /// <summary>
    /// Gets the current OAuth workflow state. // To Do: Change to OAuth
    /// </summary>
    public ObservableState<ProfileState> State { get; }

    /// <summary>
    /// Gets the linked OAuth accounts for the current user. // To Do: Change to OAuth
    /// </summary>
    public List<OAuthLinkDataTransferObject> OAuthLinks { get; private set; } // To Do: Change to OAuth

    /// <summary>
    /// Loads the OAuth links for the current user from the server. // To Do: Change to OAuth
    /// </summary>
    /// <returns><see langword="true"/> if loaded successfully; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> LoadOAuthLinks() // To Do: Change to OAuth
    {
        ErrorOr<List<OAuthLinkDataTransferObject>> oauthResult = await this.apiClient.GetAsync<List<OAuthLinkDataTransferObject>>(ApiEndpoints.OAuthLinks); // To Do: Change to OAuth
        if (oauthResult.IsError)
        {
            // 404 means no OAuth links exist — treat as success with empty list // To Do: Change to OAuth
            this.OAuthLinks = new List<OAuthLinkDataTransferObject>();
            return true;
        }

        this.OAuthLinks = oauthResult.Value;
        return true;
    }

    /// <summary>
    /// Links a new OAuth provider to the current account. // To Do: Change to OAuth
    /// </summary>
    /// <param name="provider">The provider to link.</param>
    /// <returns><see langword="true"/> if the provider was linked; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> LinkOAuth(string provider) // To Do: Change to OAuth
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

        string trimmedProvider = provider.Trim();
        var request = new { Provider = trimmedProvider };
        ErrorOr<Success> result = await this.apiClient.PostAsync(ApiEndpoints.LinkOAuth, request); // To Do: Change to OAuth

        return result.Match(
            _ =>
            {
                this.OAuthLinks.Add(new OAuthLinkDataTransferObject { Provider = trimmedProvider });
                this.State.SetValue(ProfileState.UpdateSuccess);
                return true;
            },
            errors =>
            {
                this.logger.LogError("LinkOAuth failed: {Errors}", errors); // To Do: Change to OAuth
                this.State.SetValue(ProfileState.Error);
                return false;
            });
    }

    /// <summary>
    /// Removes a linked OAuth provider from the local profile state. // To Do: Change to OAuth
    /// </summary>
    /// <param name="provider">The provider to remove.</param>
    /// <returns><see langword="true"/> if the provider was removed; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> UnlinkOAuth(string provider) // To Do: Change to OAuth
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            return false;
        }

        OAuthLinkDataTransferObject? existing = this.OAuthLinks.Find(o =>
            string.Equals(o.Provider, provider, StringComparison.OrdinalIgnoreCase));
        if (existing == null)
        {
            return false;
        }

        this.State.SetValue(ProfileState.Loading);
        ErrorOr<Success> result = await this.apiClient.DeleteAsync($"{ApiEndpoints.UnlinkOAuth}/{Uri.EscapeDataString(provider.Trim())}"); // To Do: Change to OAuth
        if (result.IsError)
        {
            this.logger.LogError("UnlinkOAuth failed: {Errors}", result.Errors); // To Do: Change to OAuth
            this.State.SetValue(ProfileState.Error);
            return false;
        }

        this.OAuthLinks.Remove(existing);
        this.State.SetValue(ProfileState.UpdateSuccess);
        return true;
    }
}
