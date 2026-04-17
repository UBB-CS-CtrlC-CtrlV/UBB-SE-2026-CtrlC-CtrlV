// <copyright file="ProfileViewModel.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;
using BankApp.Desktop.Utilities;
using BankApp.Application.DataTransferObjects.Profile;
using BankApp.Desktop.Enums;
using BankApp.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace BankApp.Desktop.ViewModels;

/// <summary>
/// Coordinates profile-related operations by delegating to specialised sub-ViewModels
/// for personal info, security, OAuth, notifications, and sessions.
/// </summary>
public class ProfileViewModel
{
    private readonly ILogger<ProfileViewModel> logger;
    private bool disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProfileViewModel"/> class.
    /// </summary>
    /// <param name="personalInfo">The personal info sub-ViewModel.</param>
    /// <param name="security">The security sub-ViewModel.</param>
    /// <param name="oAuth">The OAuth sub-ViewModel.</param>
    /// <param name="notifications">The notifications sub-ViewModel.</param>
    /// <param name="sessions">The sessions sub-ViewModel.</param>
    /// <param name="logger">Logger for profile coordination errors.</param>
    public ProfileViewModel(
        PersonalInfoViewModel personalInfo,
        SecurityViewModel security,
        OAuthViewModel oAuth,
        NotificationsViewModel notifications,
        SessionsViewModel sessions,
        ILogger<ProfileViewModel> logger)
    {
        this.PersonalInfo = personalInfo ?? throw new ArgumentNullException(nameof(personalInfo));
        this.Security = security ?? throw new ArgumentNullException(nameof(security));
        this.OAuth = oAuth ?? throw new ArgumentNullException(nameof(oAuth));
        this.Notifications = notifications ?? throw new ArgumentNullException(nameof(notifications));
        this.Sessions = sessions ?? throw new ArgumentNullException(nameof(sessions));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.State = new ObservableState<ProfileState>(ProfileState.Idle);
    }

    /// <summary>
    /// Gets the current profile workflow state.
    /// </summary>
    public ObservableState<ProfileState> State { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the View is currently initializing UI controls
    /// programmatically and toggle-changed events should be suppressed.
    /// </summary>
    public bool IsInitializingView { get; set; }

    /// <summary>
    /// Gets the personal info sub-ViewModel.
    /// </summary>
    public PersonalInfoViewModel PersonalInfo { get; }

    /// <summary>
    /// Gets the security sub-ViewModel.
    /// </summary>
    public SecurityViewModel Security { get; }

    /// <summary>
    /// Gets the OAuth sub-ViewModel.
    /// </summary>
    public OAuthViewModel OAuth { get; }

    /// <summary>
    /// Gets the notifications sub-ViewModel.
    /// </summary>
    public NotificationsViewModel Notifications { get; }

    /// <summary>
    /// Gets the sessions sub-ViewModel.
    /// </summary>
    public SessionsViewModel Sessions { get; }

    /// <summary>
    /// Gets the current user's profile details (convenience accessor).
    /// </summary>
    public ProfileInfo ProfileInfo => this.PersonalInfo.ProfileInfo;

    /// <summary>
    /// Gets a value indicating whether phone-based two-factor authentication is active.
    /// </summary>
    public bool IsPhoneTwoFactorActive =>
        this.ProfileInfo.Is2FactorAuthentificationEnabled &&
        string.Equals(this.ProfileInfo.Preferred2FAMethod, nameof(TwoFactorMethod.Phone), StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a value indicating whether email-based two-factor authentication is active.
    /// </summary>
    public bool IsEmailTwoFactorActive =>
        this.ProfileInfo.Is2FactorAuthentificationEnabled &&
        string.Equals(this.ProfileInfo.Preferred2FAMethod, nameof(TwoFactorMethod.Email), StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Loads the current user's profile, OAuth links, and notification preferences.
    /// Each request is issued sequentially; the load stops at the first failure.
    /// </summary>
    /// <returns><see langword="true"/> if all data loaded successfully; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> LoadProfile()
    {
        this.State.SetValue(ProfileState.Loading);

        if (!await this.PersonalInfo.LoadProfile())
        {
            this.State.SetValue(ProfileState.Error);
            return false;
        }

        if (!await this.OAuth.LoadOAuthLinks())
        {
            this.State.SetValue(ProfileState.Error);
            return false;
        }

        if (!await this.Notifications.LoadNotificationPreferences())
        {
            this.State.SetValue(ProfileState.Error);
            return false;
        }

        this.State.SetValue(ProfileState.UpdateSuccess);
        return true;
    }

    /// <summary>
    /// Enables two-factor authentication and updates the local profile state when successful.
    /// </summary>
    /// <param name="method">The two-factor delivery method to enable.</param>
    /// <returns><see langword="true"/> if two-factor authentication was enabled; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> EnableTwoFactor(TwoFactorMethod method)
    {
        bool success = await this.Security.EnableTwoFactor(method);
        if (!success)
        {
            return false;
        }

        this.ProfileInfo.Is2FactorAuthentificationEnabled = true;
        this.ProfileInfo.Preferred2FAMethod = method.ToString();
        return true;
    }

    /// <summary>
    /// Disables two-factor authentication and updates the local profile state when successful.
    /// </summary>
    /// <returns><see langword="true"/> if two-factor authentication was disabled; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> DisableTwoFactor()
    {
        bool success = await this.Security.DisableTwoFactor();
        if (!success)
        {
            return false;
        }

        this.ProfileInfo.Is2FactorAuthentificationEnabled = false;
        this.ProfileInfo.Preferred2FAMethod = null;
        return true;
    }

    /// <summary>
    /// Sets email two-factor authentication from the profile toggle and updates local state.
    /// </summary>
    /// <param name="enabled"><see langword="true"/> to enable email two-factor authentication; otherwise, disable it.</param>
    /// <returns><see langword="true"/> if the setting was updated; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> SetEmailTwoFactorEnabled(bool enabled)
    {
        bool success = await this.Security.SetTwoFactorEnabled(enabled);
        if (!success)
        {
            return false;
        }

        this.ProfileInfo.Is2FactorAuthentificationEnabled = enabled;
        this.ProfileInfo.Preferred2FAMethod = enabled ? nameof(TwoFactorMethod.Email) : null;
        return true;
    }

    /// <summary>
    /// Toggles a notification preference and lets the notification model roll back on failure.
    /// </summary>
    /// <param name="preference">The preference to toggle.</param>
    /// <param name="enabled">The new enabled value.</param>
    /// <returns><see langword="true"/> if the preference was saved; otherwise, <see langword="false"/>.</returns>
    public Task<bool> ToggleNotificationPreference(NotificationPreferenceDataTransferObject preference, bool enabled)
    {
        return this.Notifications.ToggleNotificationPreference(preference, enabled);
    }

    /// <summary>
    /// Loads sessions for the currently loaded user.
    /// </summary>
    /// <returns>A result indicating whether sessions were loaded and why loading may have failed.</returns>
    public async Task<(bool Success, string? ErrorMessage)> LoadSessionsForCurrentUser()
    {
        int? userId = this.ProfileInfo.UserId;
        if (userId == null)
        {
            return (false, "User not loaded.");
        }

        bool loaded = await this.Sessions.LoadSessionsAsync(userId.Value);
        return loaded ? (true, null) : (false, "Failed to load active sessions.");
    }

    /// <summary>
    /// Revokes a session and reloads the current user's active sessions.
    /// </summary>
    /// <param name="sessionId">The identifier of the session to revoke.</param>
    /// <returns>A result indicating whether the revoke and reload flow completed.</returns>
    public async Task<(bool Success, string? ErrorMessage)> RevokeSessionAndReload(int sessionId)
    {
        bool revoked = await this.Sessions.RevokeSessionAsync(sessionId);
        if (!revoked)
        {
            return (false, "Failed to revoke session.");
        }

        (bool loaded, string? errorMessage) = await this.LoadSessionsForCurrentUser();
        return loaded ? (true, null) : (false, errorMessage);
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
