// <copyright file="NotificationsViewModel.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BankApp.Desktop.Utilities;
using BankApp.Application.DataTransferObjects.Profile;
using BankApp.Desktop.Enums;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace BankApp.Desktop.ViewModels;

/// <summary>
/// Handles loading and updating notification preferences for the current user.
/// </summary>
public class NotificationsViewModel
{
    private readonly IApiClient apiClient;
    private readonly ILogger<NotificationsViewModel> logger;

    /// <summary>
    ///
    /// </summary>
    /// <param name="preference"></param>
    /// <param name="enabled"></param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    public async Task<bool> ToggleNotificationPreference(NotificationPreferenceDataTransferObject preference, bool enabled)
    {
        bool previousValue = preference.EmailEnabled;
        preference.EmailEnabled = enabled;
        bool success = await this.UpdateNotificationPreferences(this.NotificationPreferences);
        if (!success)
        {
            preference.EmailEnabled = previousValue;
        }

        return success;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationsViewModel"/> class.
    /// </summary>
    /// <param name="apiClient">The API client used for notification operations.</param>
    /// <param name="logger">Logger for notification operation errors.</param>
    public NotificationsViewModel(IApiClient apiClient, ILogger<NotificationsViewModel> logger)
    {
        this.apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.State = new ObservableState<ProfileState>(ProfileState.Idle);
        this.NotificationPreferences = new List<NotificationPreferenceDataTransferObject>();
    }

    /// <summary>
    /// Gets the current notifications workflow state.
    /// </summary>
    public ObservableState<ProfileState> State { get; }

    /// <summary>
    /// Gets the notification preferences for the current user.
    /// </summary>
    public List<NotificationPreferenceDataTransferObject> NotificationPreferences { get; private set; }

    /// <summary>
    /// Loads notification preferences for the current user from the server.
    /// </summary>
    /// <returns><see langword="true"/> if loaded successfully; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> LoadNotificationPreferences()
    {
        var preferencesResult = await this.apiClient.GetAsync<List<NotificationPreferenceDataTransferObject>>(ApiEndpoints.NotificationPreferences);
        if (preferencesResult.IsError)
        {
            this.logger.LogError("LoadNotificationPreferences: request failed: {Errors}", preferencesResult.Errors);
            return false;
        }

        this.NotificationPreferences = preferencesResult.Value;
        return true;
    }

    /// <summary>
    /// Updates notification preferences for the current user.
    /// </summary>
    /// <param name="preferences">The preferences to persist.</param>
    /// <returns><see langword="true"/> if the preferences were updated; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> UpdateNotificationPreferences(List<NotificationPreferenceDataTransferObject> preferences)
    {
        if (preferences.Count == default)
        {
            return false;
        }

        this.State.SetValue(ProfileState.Loading);

        ErrorOr<Success> result = await this.apiClient.PutAsync<List<NotificationPreferenceDataTransferObject>>(ApiEndpoints.NotificationPreferences, preferences);

        return result.Match(
            _ =>
            {
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
