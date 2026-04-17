// <copyright file="SessionsViewModel.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BankApp.Desktop.Utilities;
using BankApp.Desktop.Enums;
using BankApp.Application.DataTransferObjects.Profile;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace BankApp.Desktop.ViewModels;

/// <summary>
/// Handles active session management for the current user.
/// </summary>
public class SessionsViewModel
{
    private readonly IApiClient apiClient;
    private readonly ILogger<SessionsViewModel> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionsViewModel"/> class.
    /// </summary>
    /// <param name="apiClient">The API client used for session operations.</param>
    /// <param name="logger">Logger for session operation errors.</param>
    public SessionsViewModel(IApiClient apiClient, ILogger<SessionsViewModel> logger)
    {
        this.apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.State = new ObservableState<ProfileState>(ProfileState.Idle);
        this.ActiveSessions = new List<SessionDataTransferObject>();
    }

    /// <summary>
    /// Gets the current sessions workflow state.
    /// </summary>
    public ObservableState<ProfileState> State { get; }

    /// <summary>
    /// Gets the active sessions for the current user.
    /// </summary>
    public List<SessionDataTransferObject> ActiveSessions { get; private set; }

    /// <summary>
    /// Loads all active sessions for the specified user from the server.
    /// </summary>
    /// <param name="userId">The identifier of the current user.</param>
    /// <returns><see langword="true"/> if sessions loaded successfully; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> LoadSessionsAsync(int userId)
    {
        this.State.SetValue(ProfileState.Loading);
        try
        {
            ErrorOr<List<SessionDataTransferObject>> result = await this.apiClient.GetAsync<List<SessionDataTransferObject>>(ApiEndpoints.Sessions);
            if (result.IsError)
            {
                this.ActiveSessions = new List<SessionDataTransferObject>();
                this.State.SetValue(ProfileState.Error);
                return false;
            }

            this.ActiveSessions = result.Value;
            this.State.SetValue(ProfileState.Idle);
            return true;
        }
        catch (Exception exception)
        {
            this.logger.LogError(exception, "Failed to load sessions for user {UserId}", userId);
            this.ActiveSessions = new List<SessionDataTransferObject>();
            this.State.SetValue(ProfileState.Error);
            return false;
        }
    }

    /// <summary>
    /// Revokes a specific session by its identifier.
    /// </summary>
    /// <param name="sessionId">The identifier of the session to revoke.</param>
    /// <returns><see langword="true"/> if the session was revoked successfully; otherwise <see langword="false"/>.</returns>
    public async Task<bool> RevokeSessionAsync(int sessionId)
    {
        this.State.SetValue(ProfileState.Loading);
        try
        {
            ErrorOr<Success> result = await this.apiClient.DeleteAsync($"{ApiEndpoints.Sessions}/{sessionId}");
            this.State.SetValue(result.IsError ? ProfileState.Error : ProfileState.Idle);
            return !result.IsError;
        }
        catch (Exception exception)
        {
            this.logger.LogError(exception, "Failed to revoke session {SessionId}", sessionId);
            this.State.SetValue(ProfileState.Error);
            return false;
        }
    }
}
