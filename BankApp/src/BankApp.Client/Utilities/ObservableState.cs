using System.Collections.Generic;

namespace BankApp.Client.Utilities
{
    public class ObservableState<T>
    {
        public T Value { get; private set; }
        private readonly List<IStateObserver<T>> _observers;

        public ObservableState(T value)
        {
            _observers = new List<IStateObserver<T>>();
            Value = value;
        }

        public void SetValue(T value)
        {
            Value = value;
            NotifyObservers();
        }

        public void AddObserver(IStateObserver<T> observer)
        {
            _observers.Add(observer);
        }

        public void RemoveObserver(IStateObserver<T> observer)
        {
            _observers.Remove(observer);
        }

        private void NotifyObservers()
        {
            foreach (var observer in _observers)
            {
                observer.Update(Value);
            }
        }
    }
}

