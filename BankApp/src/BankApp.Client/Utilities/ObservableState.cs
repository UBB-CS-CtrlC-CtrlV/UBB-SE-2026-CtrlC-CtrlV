using System.Collections.Generic;

namespace BankApp.Client.Utilities;

public class ObservableState<T>(T value)
{
    private readonly List<IStateObserver<T>> observers =
    [
    ];

    public T Value { get; private set; } = value;

    public void SetValue(T value)
    {
        this.Value = value;
        this.NotifyObservers();
    }

    public void AddObserver(IStateObserver<T> observer)
    {
        this.observers.Add(observer);
    }

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