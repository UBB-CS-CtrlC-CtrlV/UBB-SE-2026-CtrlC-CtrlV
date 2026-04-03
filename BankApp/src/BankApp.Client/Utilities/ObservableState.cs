using System.Collections.Generic;

namespace BankApp.Client.Utilities;

/// <summary>
/// Simple observable state class that allows observers to
/// subscribe and be notified when the state changes.
/// </summary>
/// <param name="value">Value to be observed.</param>
/// <typeparam name="T"></typeparam>
public class ObservableState<T>(T value)
{
    private readonly List<IStateObserver<T>> observers =
    [
    ];

    /// <summary>
    /// Gets the current value of the state.
    /// Observers will be notified whenever this value changes via the SetValue method.
    /// </summary>
    public T Value { get; private set; } = value;

    /// <summary>
    /// Sets a new value for the state and notifies all subscribed observers of the change.
    /// </summary>
    /// <param name="value"></param>
    public void SetValue(T value)
    {
        this.Value = value;
        this.NotifyObservers();
    }

    /// <summary>
    /// Adds an observer to the list of observers that will be notified when the state changes.
    /// </summary>
    /// <param name="observer"></param>
    public void AddObserver(IStateObserver<T> observer)
    {
        this.observers.Add(observer);
    }

    /// <summary>
    /// Removes an observer from the list of observers.
    /// The removed observer will no longer receive notifications when the state changes.
    /// </summary>
    /// <param name="observer"></param>
    public void RemoveObserver(IStateObserver<T> observer)
    {
        this.observers.Remove(observer);
    }

    private void NotifyObservers()
    {
        foreach (var observer in this.observers)
        {
            observer.Update(this.Value);
        }
    }
}