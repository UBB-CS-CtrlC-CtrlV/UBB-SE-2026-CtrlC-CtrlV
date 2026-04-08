// <copyright file="SessionsViewModel.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using BankApp.Client.Utilities;
using BankApp.Contracts.Entities;
using BankApp.Client.Enums;
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
}
