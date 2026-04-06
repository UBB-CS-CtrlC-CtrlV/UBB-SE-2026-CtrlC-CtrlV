// <copyright file="ObservableViewBase.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

namespace BankApp.Client.Utilities;

/// <summary>
/// Observable view base class that implements the IStateObserver interface.
/// </summary>
/// <typeparam name="T">The type of state that triggers view updates.</typeparam>
public abstract class ObservableViewBase<T> : IStateObserver<T>
{
    /// <inheritdoc />
    public void Update(T value)
    {
        OnStateChanged(value);
    }

    /// <summary>
    /// Called when the state changes.
    /// Implement this method to update the view based on the new state.
    /// </summary>
    /// <param name="state">The new state value.</param>
    public abstract void OnStateChanged(T state);

    /// <summary>
    /// Shows an error message to the user.
    /// </summary>
    /// <param name="msg">The error message to display.</param>
    public abstract void ShowError(string msg);

    /// <summary>
    /// Shows a loading indicator to the user, indicating that a background operation is in progress.
    /// </summary>
    public abstract void ShowLoading();

    /// <summary>
    /// Hides the loading indicator from the user interface.
    /// </summary>
    /// <remarks>Call this method to remove or conceal any visual indication of a loading or
    /// processing state. The specific behavior depends on the implementation in a derived class.
    /// </remarks>
    public abstract void HideLoading();
}
