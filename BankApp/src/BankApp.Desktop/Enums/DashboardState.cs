// <copyright file="DashboardState.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

namespace BankApp.Desktop.Enums;

/// <summary>
/// Represents the possible states of the dashboard loading flow.
/// </summary>
public enum DashboardState
{
    /// <summary>
    /// No data load has been triggered yet.
    /// </summary>
    Idle,

    /// <summary>
    /// Dashboard data is being fetched from the server.
    /// </summary>
    Loading,

    /// <summary>
    /// Dashboard data was fetched successfully and is ready to display.
    /// </summary>
    Success,

    /// <summary>
    /// An error occurred while loading dashboard data.
    /// </summary>
    Error,
}
