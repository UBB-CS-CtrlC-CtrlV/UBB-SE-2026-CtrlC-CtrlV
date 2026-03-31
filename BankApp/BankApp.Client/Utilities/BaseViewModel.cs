using System;

namespace BankApp.Client.Utilities
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public void OnPropertyChanged(string propertyName)
        {
            //throw new NotImplementedException();
        }

        protected void SetState<T>(Observable<T> observable, T value)
        {
            observable.SetValue(value);
        }
        public abstract void Dispose();
    }
}
