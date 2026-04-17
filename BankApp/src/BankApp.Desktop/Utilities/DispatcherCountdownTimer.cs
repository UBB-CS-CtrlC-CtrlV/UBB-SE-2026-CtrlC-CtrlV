// <copyright file="DispatcherCountdownTimer.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System;
using Microsoft.UI.Xaml;

namespace BankApp.Desktop.Utilities;

/// <summary>
/// Production implementation of <see cref="ICountdownTimer"/> backed by
/// <see cref="DispatcherTimer"/>. Because <see cref="DispatcherTimer"/> runs on the
/// UI thread, <see cref="Tick"/> events are always raised on the UI thread, which
/// makes it safe for ViewModels to update observable properties from the handler
/// without extra marshalling.
/// </summary>
public sealed class DispatcherCountdownTimer : ICountdownTimer
{
    private const int TimerIntervalSeconds = 1;

    private readonly DispatcherTimer inner;

    /// <summary>
    /// Initializes a new instance of the <see cref="DispatcherCountdownTimer"/> class
    /// with a one-second interval.
    /// </summary>
    public DispatcherCountdownTimer()
    {
        this.inner = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(TimerIntervalSeconds),
        };

        this.inner.Tick += (_, _) => this.Tick?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc/>
    public event EventHandler? Tick;

    /// <inheritdoc/>
    public void Start()
    {
        this.inner.Start();
    }

    /// <inheritdoc/>
    public void Stop()
    {
        this.inner.Stop();
    }
}
