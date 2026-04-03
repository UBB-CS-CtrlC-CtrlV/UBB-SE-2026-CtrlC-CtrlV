namespace BankApp.Client.Utilities
{
    /// <summary>
    /// Simple observer interface for observing changes in a state of type T.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IStateObserver<in T>
    {
        /// <summary>
        /// Updates the observer with a new value of type T,
        /// allowing it to react to changes in the observed state.
        /// </summary>
        /// <param name="value"></param>
        void Update(T value);
    }
}

