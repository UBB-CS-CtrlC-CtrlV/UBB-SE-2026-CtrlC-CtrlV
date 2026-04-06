// <copyright file="SessionsViewModel.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BankApp.Client.Utilities;
using BankApp.Core.Entities;
using BankApp.Core.Enums;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace BankApp.Client.ViewModels;

/// <summary>
/// Handles active session management for the current user.
/// </summary>
public class SessionsViewModel
{
    private readonly ApiClient apiClient;
    private readonly ILogger<SessionsViewModel> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionsViewModel"/> class.
    /// </summary>
    /// <param name="apiClient">The API client used for session operations.</param>
    /// <param name="logger">Logger for session operation errors.</param>
    public SessionsViewModel(ApiClient apiClient, ILogger<SessionsViewModel> logger)
    {
        this.apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.State = new ObservableState<ProfileState>(ProfileState.Idle);
        this.ActiveSessions = new List<Session>();
    }

    /// <summary>
    /// Gets the current sessions workflow state.
    /// </summary>
    public ObservableState<ProfileState> State { get; }

    /// <summary>
    /// Gets the active sessions for the current user.
    /// </summary>
    public List<Session> ActiveSessions { get; private set; }

    /// <summary>
    /// Loads all active sessions for the specified user from the server.
    /// </summary>
    /// <param name="userId">The identifier of the current user.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task LoadSessionsAsync(int userId)
    {
        this.State.SetValue(ProfileState.Loading);
        try
        {
            ErrorOr<List<Session>> result = await this.apiClient.GetAsync<List<Session>>("/api/profile/sessions");
            this.ActiveSessions = result.IsError ? new List<Session>() : result.Value;
            this.State.SetValue(result.IsError ? ProfileState.Error : ProfileState.Idle);
        }
        catch (Exception exception)
        {
            this.logger.LogError(exception, "Failed to load sessions for user {UserId}", userId);
            this.ActiveSessions = new List<Session>();
            this.State.SetValue(ProfileState.Error);
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
            ErrorOr<bool> result = await this.apiClient.DeleteAsync<bool>($"/api/profile/sessions/{sessionId}");
            this.State.SetValue(result.IsError ? ProfileState.Error : ProfileState.Idle);
            return !result.IsError && result.Value;
        }
        catch (Exception exception)
        {
            this.logger.LogError(exception, "Failed to revoke session {SessionId}", sessionId);
            this.State.SetValue(ProfileState.Error);
            return false;
        }
    }
}