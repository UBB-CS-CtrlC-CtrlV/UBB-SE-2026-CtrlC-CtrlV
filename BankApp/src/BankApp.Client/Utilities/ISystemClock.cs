// <copyright file="ISystemClock.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System;

namespace BankApp.Client.Utilities;

/// <summary>
/// Abstracts the system clock to allow deterministic time-based testing.
/// </summary>
public interface ISystemClock
{
    /// <summary>
    /// Gets the current UTC time.
    /// </summary>
    DateTime UtcNow { get; }
}
