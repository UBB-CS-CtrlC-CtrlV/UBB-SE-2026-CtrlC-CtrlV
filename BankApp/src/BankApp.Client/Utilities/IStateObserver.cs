namespace BankApp.Client.Utilities
{
    public interface IStateObserver<T>
    {
        void Update(T value);
    }
}

