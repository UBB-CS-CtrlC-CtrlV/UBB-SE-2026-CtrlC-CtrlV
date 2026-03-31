using Microsoft.UI.Xaml.Controls;

namespace BankApp.Client.Master
{
    public interface IAppNavigationService
    {
        void SetFrame(Frame frame);
        void SetContentFrame(Frame frame);
        void NavigateTo<TPage>();
        void NavigateToContent<TPage>();
        void GoBack();
        bool CanGoBack();
    }
}

