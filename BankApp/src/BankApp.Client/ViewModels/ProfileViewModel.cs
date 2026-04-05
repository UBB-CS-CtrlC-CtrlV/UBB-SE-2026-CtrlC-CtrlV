// <copyright file="ProfileViewModel.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BankApp.Client.Utilities;
using BankApp.Core.DTOs.Profile;
using BankApp.Core.Entities;
using BankApp.Core.Enums;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace BankApp.Client.ViewModels;

/// <summary>
/// Coordinates profile-related API operations and exposes profile data to the view.
/// </summary>
public class ProfileViewModel
{
    private readonly ApiClient apiClient;
    private readonly ILogger<ProfileViewModel> logger;
    private bool disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProfileViewModel"/> class.
    /// </summary>
    /// <param name="apiClient">The API client used for profile operations.</param>
    /// <param name="logger">Logger for profile operation errors.</param>
    public ProfileViewModel(ApiClient apiClient, ILogger<ProfileViewModel> logger)
    {
        this.apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.State = new ObservableState<ProfileState>(ProfileState.Idle);
        this.ProfileInfo = new ProfileInfo();
        this.OAuthLinks = new List<OAuthLink>();
        this.ActiveSessions = new List<Session>();
        this.NotificationPreferences = new List<NotificationPreference>();
    }

    /// <summary>
    /// Gets the current profile workflow state.
    /// </summary>
    public ObservableState<ProfileState> State { get; }

    /// <summary>
    /// Gets the current user's profile details.
    /// </summary>
    public ProfileInfo ProfileInfo { get; private set; }

    /// <summary>
    /// Gets the linked OAuth accounts for the current user.
    /// </summary>
    public List<OAuthLink> OAuthLinks { get; private set; }

    /// <summary>
    /// Gets the active sessions for the current user.
    /// </summary>
    public List<Session> ActiveSessions { get; private set; }

    /// <summary>
    /// Gets the notification preferences for the current user.
    /// </summary>
    public List<NotificationPreference> NotificationPreferences { get; private set; }

    /// <summary>
    /// Loads the current user's profile, OAuth links, and notification preferences.
    /// Each request is issued sequentially; the load stops at the first failure.
    /// </summary>
    /// <returns><see langword="true"/> if all data loaded successfully; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> LoadProfile()
    {
        this.State.SetValue(ProfileState.Loading);

        var profileResult = await this.apiClient.GetAsync<GetProfileResponse>("api/profile/");
        if (profileResult.IsError)
        {
            this.logger.LogError("LoadProfile: profile request failed: {Errors}", profileResult.Errors);
            this.State.SetValue(ProfileState.Error);
            return false;
        }

        if (!profileResult.Value.Success || profileResult.Value.ProfileInfo == null)
        {
            this.State.SetValue(ProfileState.Error);
            return false;
        }

        var oauthResult = await this.apiClient.GetAsync<List<OAuthLink>>("api/profile/oauthlinks");
        if (oauthResult.IsError)
        {
            this.logger.LogError("LoadProfile: OAuth links request failed: {Errors}", oauthResult.Errors);
            this.State.SetValue(ProfileState.Error);
            return false;
        }

        var prefsResult = await this.apiClient.GetAsync<List<NotificationPreference>>("api/profile/notifications/preferences");
        if (prefsResult.IsError)
        {
            this.logger.LogError("LoadProfile: notification preferences request failed: {Errors}", prefsResult.Errors);
            this.State.SetValue(ProfileState.Error);
            return false;
        }

        this.ProfileInfo = profileResult.Value.ProfileInfo;
        this.OAuthLinks = oauthResult.Value;
        this.NotificationPreferences = prefsResult.Value;
        this.State.SetValue(ProfileState.UpdateSuccess);
        return true;
    }

    /// <summary>
    /// Updates the user's phone number and address.
    /// </summary>
    /// <param name="phone">The phone number to persist.</param>
    /// <param name="address">The address to persist.</param>
    /// <param name="password">The verified password associated with the edit flow.</param>
    /// <returns><see langword="true"/> if the update succeeded; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> UpdatePersonalInfo(string? phone, string? address, string password)
    {
        this.State.SetValue(ProfileState.Loading);

        if (this.ProfileInfo.UserId == null)
        {
            this.State.SetValue(ProfileState.Error);
            return false;
        }

        string? trimmedPhone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        string? trimmedAddress = string.IsNullOrWhiteSpace(address) ? null : address.Trim();

        var request = new UpdateProfileRequest(this.ProfileInfo.UserId, trimmedPhone, trimmedAddress);
        var result = await this.apiClient.PutAsync<UpdateProfileRequest, UpdateProfileResponse>("api/profile/", request);

        return result.Match(
            response =>
            {
                if (!response.Success)
                {
                    this.State.SetValue(ProfileState.Error);
                    return false;
                }

                this.ProfileInfo.PhoneNumber = trimmedPhone;
                this.ProfileInfo.Address = trimmedAddress;
                this.State.SetValue(ProfileState.UpdateSuccess);
                return true;
            },
            errors =>
            {
                this.logger.LogError("UpdatePersonalInfo failed: {Errors}", errors);
                this.State.SetValue(ProfileState.Error);
                return false;
            });
    }

    /// <summary>
    /// Changes the current user's password.
    /// </summary>
    /// <param name="currentPassword">The current password for verification.</param>
    /// <param name="newPassword">The new password to apply.</param>
    /// <returns><see langword="true"/> if the password changed successfully; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> ChangePassword(string currentPassword, string newPassword)
    {
        this.State.SetValue(ProfileState.Loading);

        if (this.ProfileInfo.UserId == null)
        {
            this.State.SetValue(ProfileState.Error);
            return false;
        }

        var request = new ChangePasswordRequest(this.ProfileInfo.UserId.Value, currentPassword, newPassword);
        var result = await this.apiClient.PutAsync<ChangePasswordRequest, ChangePasswordResponse>("api/profile/password", request);

        return result.Match(
            response =>
            {
                if (!response.Success)
                {
                    this.State.SetValue(ProfileState.Error);
                    return false;
                }

                this.State.SetValue(ProfileState.UpdateSuccess);
                return true;
            },
            errors =>
            {
                this.logger.LogError("ChangePassword failed: {Errors}", errors);
                this.State.SetValue(ProfileState.Error);
                return false;
            });
    }

    /// <summary>
    /// Enables two-factor authentication for the current user.
    /// </summary>
    /// <param name="method">The two-factor delivery method to enable.</param>
    /// <returns><see langword="true"/> if the setting was updated; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> EnableTwoFactor(TwoFactorMethod method)
    {
        this.State.SetValue(ProfileState.Loading);

        var request = new { Method = method };
        var result = await this.apiClient.PutAsync<object, Toggle2FAResponse>("api/profile/2fa/enable", request);

        return result.Match(
            response =>
            {
                if (!response.Success)
                {
                    this.State.SetValue(ProfileState.Error);
                    return false;
                }

                this.ProfileInfo.Is2FAEnabled = true;
                this.State.SetValue(ProfileState.UpdateSuccess);
                return true;
            },
            errors =>
            {
                this.logger.LogError("EnableTwoFactor failed: {Errors}", errors);
                this.State.SetValue(ProfileState.Error);
                return false;
            });
    }

    /// <summary>
    /// Disables two-factor authentication for the current user.
    /// </summary>
    /// <returns><see langword="true"/> if the setting was updated; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> DisableTwoFactor()
    {
        this.State.SetValue(ProfileState.Loading);

        var result = await this.apiClient.PutAsync<object, Toggle2FAResponse>("api/profile/2fa/disable", new { });

        return result.Match(
            response =>
            {
                if (!response.Success)
                {
                    this.State.SetValue(ProfileState.Error);
                    return false;
                }

                this.ProfileInfo.Is2FAEnabled = false;
                this.State.SetValue(ProfileState.UpdateSuccess);
                return true;
            },
            errors =>
            {
                this.logger.LogError("DisableTwoFactor failed: {Errors}", errors);
                this.State.SetValue(ProfileState.Error);
                return false;
            });
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

    /// <summary>
    /// Updates notification preferences for the current user.
    /// </summary>
    /// <param name="preferences">The preferences to persist.</param>
    /// <returns><see langword="true"/> if the preferences were updated; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> UpdateNotificationPreferences(List<NotificationPreference> preferences)
    {
        if (preferences.Count == 0)
        {
            return false;
        }

        this.State.SetValue(ProfileState.Loading);

        var result = await this.apiClient.PutAsync<List<NotificationPreference>, bool>("api/profile/notifications/preferences", preferences);

        return result.Match(
            updated =>
            {
                if (!updated)
                {
                    this.State.SetValue(ProfileState.Error);
                    return false;
                }

                this.NotificationPreferences = preferences;
                this.State.SetValue(ProfileState.UpdateSuccess);
                return true;
            },
            errors =>
            {
                this.logger.LogError("UpdateNotificationPreferences failed: {Errors}", errors);
                this.State.SetValue(ProfileState.Error);
                return false;
            });
    }

    /// <summary>
    /// Verifies the supplied password against the server.
    /// </summary>
    /// <param name="password">The password to verify.</param>
    /// <returns><see langword="true"/> if the password is valid; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> VerifyPassword(string password)
    {
        this.State.SetValue(ProfileState.Loading);

        if (this.ProfileInfo.UserId == null)
        {
            this.State.SetValue(ProfileState.Error);
            return false;
        }

        var result = await this.apiClient.PostAsync<string, bool>("api/profile/verify-password", password);

        return result.Match(
            valid =>
            {
                if (!valid)
                {
                    this.State.SetValue(ProfileState.Error);
                    return false;
                }

                this.State.SetValue(ProfileState.UpdateSuccess);
                return true;
            },
            errors =>
            {
                this.logger.LogError("VerifyPassword failed: {Errors}", errors);
                this.State.SetValue(ProfileState.Error);
                return false;
            });
    }

    /// <summary>
    /// Releases resources used by the view model.
    /// </summary>
    public void Dispose()
    {
        if (this.disposed)
        {
            return;
        }

        this.disposed = true;
        GC.SuppressFinalize(this);
    }
}
