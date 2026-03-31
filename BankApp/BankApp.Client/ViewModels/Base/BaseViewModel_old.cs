using System;
using System.ComponentModel;
using Windows.Foundation.Metadata;
namespace BankApp.Client.ViewModels.Base
{
    // Deprecated, at least according to the UML diagram. New version can be found in Utilities
    [Obsolete]
    public abstract class BaseViewModel_old : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
