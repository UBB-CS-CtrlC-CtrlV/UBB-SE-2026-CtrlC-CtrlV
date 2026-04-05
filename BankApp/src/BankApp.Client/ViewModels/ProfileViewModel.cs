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

namespace BankApp.Client.ViewModels;

/// <summary>
/// Coordinates profile-related API operations and exposes profile data to the view.
/// </summary>
public class ProfileViewModel
{
    private readonly ApiClient apiClient;
    private bool disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProfileViewModel"/> class.
    /// </summary>
    /// <param name="apiClient">The API client used for profile operations.</param>
    public ProfileViewModel(ApiClient apiClient)
    {
        this.apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
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
    /// </summary>
    /// <returns><see langword="true"/> if the data loaded successfully; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> LoadProfile()
    {
        try
        {
            this.State.SetValue(ProfileState.Loading);

            GetProfileResponse? profileResponse = await this.apiClient.GetAsync<GetProfileResponse>("api/profile/");
            if (profileResponse == null || !profileResponse.Success || profileResponse.ProfileInfo == null)
            {
                this.State.SetValue(ProfileState.Error);
                return false;
            }

            List<OAuthLink>? oauthResponse = await this.apiClient.GetAsync<List<OAuthLink>>("api/profile/oauthlinks");
            if (oauthResponse == null)
            {
                this.State.SetValue(ProfileState.Error);
                return false;
            }

            List<NotificationPreference>? preferencesResponse =
                await this.apiClient.GetAsync<List<NotificationPreference>>("api/profile/notifications/preferences");
            if (preferencesResponse == null)
            {
                this.State.SetValue(ProfileState.Error);
                return false;
            }

            this.ProfileInfo = profileResponse.ProfileInfo;
            this.OAuthLinks = oauthResponse;
            this.NotificationPreferences = preferencesResponse;
            this.State.SetValue(ProfileState.UpdateSuccess);
            return true;
        }
        catch (Exception ex)
        {
            this.State.SetValue(ProfileState.Error);
            this.LogError(nameof(this.LoadProfile), ex);
            return false;
        }
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
        try
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
            UpdateProfileResponse? response = await this.apiClient.PutAsync<UpdateProfileRequest, UpdateProfileResponse>("api/profile/", request);
            if (response == null)
            {
                this.State.SetValue(ProfileState.Error);
                return false;
            }

            if (!response.Success)
            {
                this.State.SetValue(ProfileState.Error);
                return false;
            }

            this.ProfileInfo.PhoneNumber = trimmedPhone;
            this.ProfileInfo.Address = trimmedAddress;
            this.State.SetValue(ProfileState.UpdateSuccess);
            return true;
        }
        catch (Exception ex)
        {
            this.State.SetValue(ProfileState.Error);
            this.LogError(nameof(this.UpdatePersonalInfo), ex);
            return false;
        }
    }

    /// <summary>
    /// Changes the current user's password.
    /// </summary>
    /// <param name="currentPassword">The current password for verification.</param>
    /// <param name="newPassword">The new password to apply.</param>
    /// <returns><see langword="true"/> if the password changed successfully; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> ChangePassword(string currentPassword, string newPassword)
    {
        try
        {
            this.State.SetValue(ProfileState.Loading);

            if (this.ProfileInfo.UserId == null)
            {
                this.State.SetValue(ProfileState.Error);
                return false;
            }

            var request = new ChangePasswordRequest(this.ProfileInfo.UserId.Value, currentPassword, newPassword);
            ChangePasswordResponse? result =
                await this.apiClient.PutAsync<ChangePasswordRequest, ChangePasswordResponse>("api/profile/password", request);
            if (result == null || !result.Success)
            {
                this.State.SetValue(ProfileState.Error);
                return false;
            }

            this.State.SetValue(ProfileState.UpdateSuccess);
            return true;
        }
        catch (Exception ex)
        {
            this.State.SetValue(ProfileState.Error);
            this.LogError(nameof(this.ChangePassword), ex);
            return false;
        }
    }

    /// <summary>
    /// Enables two-factor authentication for the current user.
    /// </summary>
    /// <param name="method">The two-factor delivery method to enable.</param>
    /// <returns><see langword="true"/> if the setting was updated; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> EnableTwoFactor(TwoFactorMethod method)
    {
        try
        {
            this.State.SetValue(ProfileState.Loading);

            var request = new { Method = method };
            Toggle2FAResponse? result = await this.apiClient.PutAsync<object, Toggle2FAResponse>("api/profile/2fa/enable", request);
            if (result?.Success != true)
            {
                this.State.SetValue(ProfileState.Error);
                return false;
            }

            this.ProfileInfo.Is2FAEnabled = true;
            this.State.SetValue(ProfileState.UpdateSuccess);
            return true;
        }
        catch (Exception ex)
        {
            this.State.SetValue(ProfileState.Error);
            this.LogError(nameof(this.EnableTwoFactor), ex);
            return false;
        }
    }

    /// <summary>
    /// Disables two-factor authentication for the current user.
    /// </summary>
    /// <returns><see langword="true"/> if the setting was updated; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> DisableTwoFactor()
    {
        try
        {
            this.State.SetValue(ProfileState.Loading);

            Toggle2FAResponse? result =
                await this.apiClient.PutAsync<object, Toggle2FAResponse>("api/profile/2fa/disable", new { });
            if (result?.Success != true)
            {
                this.State.SetValue(ProfileState.Error);
                return false;
            }

            this.ProfileInfo.Is2FAEnabled = false;
            this.State.SetValue(ProfileState.UpdateSuccess);
            return true;
        }
        catch (Exception ex)
        {
            this.State.SetValue(ProfileState.Error);
            this.LogError(nameof(this.DisableTwoFactor), ex);
            return false;
        }
    }

    /// <summary>
    /// Links a new OAuth provider to the current account.
    /// </summary>
    /// <param name="provider">The provider to link.</param>
    /// <returns><see langword="true"/> if the provider was linked; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> LinkOAuth(string provider)
    {
        try
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
            bool result = await this.apiClient.PostAsync<object, bool>("api/profile/oauth/link", request);
            if (!result)
            {
                this.State.SetValue(ProfileState.Error);
                return false;
            }

            this.State.SetValue(ProfileState.UpdateSuccess);
            return true;
        }
        catch (Exception ex)
        {
            this.State.SetValue(ProfileState.Error);
            this.LogError(nameof(this.LinkOAuth), ex);
            return false;
        }
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
        try
        {
            if (preferences.Count == 0)
            {
                return false;
            }

            this.State.SetValue(ProfileState.Loading);

            bool result = await this.apiClient.PutAsync<List<NotificationPreference>, bool>("api/profile/notifications/preferences", preferences);
            if (!result)
            {
                this.State.SetValue(ProfileState.Error);
                return false;
            }

            this.NotificationPreferences = preferences;
            this.State.SetValue(ProfileState.UpdateSuccess);
            return true;
        }
        catch (Exception ex)
        {
            this.State.SetValue(ProfileState.Error);
            this.LogError(nameof(this.UpdateNotificationPreferences), ex);
            return false;
        }
    }

    /// <summary>
    /// Verifies the supplied password against the server.
    /// </summary>
    /// <param name="password">The password to verify.</param>
    /// <returns><see langword="true"/> if the password is valid; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> VerifyPassword(string password)
    {
        try
        {
            this.State.SetValue(ProfileState.Loading);

            if (this.ProfileInfo.UserId == null)
            {
                this.State.SetValue(ProfileState.Error);
                return false;
            }

            bool? response = await this.apiClient.PostAsync<string, bool>("api/profile/verify-password", password);
            bool result = response ?? false;
            if (!result)
            {
                this.State.SetValue(ProfileState.Error);
                return false;
            }

            this.State.SetValue(ProfileState.UpdateSuccess);
            return true;
        }
        catch (Exception ex)
        {
            this.State.SetValue(ProfileState.Error);
            this.LogError(nameof(this.VerifyPassword), ex);
            return false;
        }
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

    private void LogError(string method, Exception ex)
    {
        Console.Error.WriteLine($"[ProfileViewModel] {method}: {ex.Message}");
    }
}
