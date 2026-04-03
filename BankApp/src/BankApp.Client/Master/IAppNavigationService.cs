using Microsoft.UI.Xaml.Controls;

namespace BankApp.Client.Master
{
    /// <summary>
    /// Navigation service interface for the application,
    /// defining methods for setting navigation frames.
    /// </summary>
    public interface IAppNavigationService
    {
        /// <summary>
        /// Sets the main navigation frame for the application,
        /// which will be used for primary page navigation.
        /// </summary>
        /// <param name="frame"></param>
        void SetFrame(Frame frame);

        /// <summary>
        /// Sets the content frame used to display navigation content,
        /// allowing for separation of main navigation and content display.
        /// </summary>
        /// <param name="frame"></param>
        void SetContentFrame(Frame frame);

        /// <summary>
        /// Navigates to a specified page type within the main navigation frame.
        /// </summary>
        /// <typeparam name="TPage"></typeparam>
        void NavigateTo<TPage>();

        /// <summary>
        /// Navigates to a specified page type within the content frame.
        /// </summary>
        /// <typeparam name="TPage"></typeparam>
        void NavigateToContent<TPage>();

        /// <summary>
        /// Goes back to the previous page in the navigation history of the main frame, if possible.
        /// </summary>
        void GoBack();

        /// <summary>
        /// Checks if backward navigation is possible in the main frame's navigation history.
        /// </summary>
        /// <returns></returns>
        bool CanGoBack();
    }
}

