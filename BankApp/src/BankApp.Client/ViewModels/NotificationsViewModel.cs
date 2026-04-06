// <copyright file="NotificationsViewModel.cs" company="CtrlC CtrlV">
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
/// Handles loading and updating notification preferences for the current user.
/// </summary>
public class NotificationsViewModel
{
    private readonly ApiClient apiClient;
    private readonly ILogger<NotificationsViewModel> logger;

    /// <summary>
    ///
    /// </summary>
    /// <param name="preference"></param>
    /// <param name="enabled"></param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    public async Task<bool> ToggleNotificationPreference(NotificationPreference preference, bool enabled)
    {
        preference.EmailEnabled = enabled;
        return await this.UpdateNotificationPreferences(this.NotificationPreferences);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationsViewModel"/> class.
    /// </summary>
    /// <param name="apiClient">The API client used for notification operations.</param>
    /// <param name="logger">Logger for notification operation errors.</param>
    public NotificationsViewModel(ApiClient apiClient, ILogger<NotificationsViewModel> logger)
    {
        this.apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.State = new ObservableState<ProfileState>(ProfileState.Idle);
        this.NotificationPreferences = new List<NotificationPreference>();
    }

    /// <summary>
    /// Gets the current notifications workflow state.
    /// </summary>
    public ObservableState<ProfileState> State { get; }

    /// <summary>
    /// Gets the notification preferences for the current user.
    /// </summary>
    public List<NotificationPreference> NotificationPreferences { get; private set; }

    /// <summary>
    /// Loads notification preferences for the current user from the server.
    /// </summary>
    /// <returns><see langword="true"/> if loaded successfully; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> LoadNotificationPreferences()
    {
        var prefsResult = await this.apiClient.GetAsync<List<NotificationPreference>>("api/profile/notifications/preferences");
        if (prefsResult.IsError)
        {
            this.logger.LogError("LoadNotificationPreferences: request failed: {Errors}", prefsResult.Errors);
            return false;
        }

        this.NotificationPreferences = prefsResult.Value;
        return true;
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
}
