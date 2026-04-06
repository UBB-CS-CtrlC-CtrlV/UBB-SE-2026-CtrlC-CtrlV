// <copyright file="ProfileViewModel.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;
using BankApp.Client.Utilities;
using BankApp.Core.DTOs.Profile;
using BankApp.Client.Enums;
using Microsoft.Extensions.Logging;

namespace BankApp.Client.ViewModels;

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
