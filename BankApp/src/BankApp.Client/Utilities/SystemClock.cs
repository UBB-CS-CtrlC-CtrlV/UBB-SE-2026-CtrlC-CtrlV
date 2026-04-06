// <copyright file="SystemClock.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System;

namespace BankApp.Client.Utilities;

/// <summary>
/// Production implementation of <see cref="ISystemClock"/> backed by <see cref="DateTime.UtcNow"/>.
/// </summary>
public class SystemClock : ISystemClock
{
    /// <inheritdoc/>
    public DateTime UtcNow => DateTime.UtcNow;
}
