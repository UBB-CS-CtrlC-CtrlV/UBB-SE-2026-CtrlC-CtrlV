using Microsoft.UI.Xaml.Controls;

namespace BankApp.Client.Master
{
    /// <summary>
    /// Provides navigation services for the application, enabling switching between pages and managing navigation
    /// history.
    /// </summary>
    /// <remarks>This service abstracts navigation logic for the application's main and content frames. It
    /// allows for type-safe navigation to pages and supports checking and performing backward navigation.
    /// </remarks>
    public class AppNavigationService : IAppNavigationService
    {
        private Frame? frame;
        private Frame? contentFrame;

        /// <summary>
        /// Sets the main navigation frame for the application.
        /// </summary>
        /// <param name="frame"></param>
        public void SetFrame(Frame frame)
        {
            this.frame = frame;
        }

        /// <summary>
        /// Sets the content frame used to display navigation content.
        /// </summary>
        /// <param name="frame">The frame that will be used to host and display navigable content. Cannot be null.</param>
        public void SetContentFrame(Frame frame)
        {
            this.contentFrame = frame;
        }

        /// <summary>
        /// Navigates to a specified page type within the main navigation frame.
        /// </summary>
        /// <typeparam name="TPage"></typeparam>
        public void NavigateTo<TPage>()
        {
            this.frame?.Navigate(typeof(TPage));
        }

        /// <summary>
        /// Navigates to a specified page type within the content frame,
        /// allowing for separation of main navigation.
        /// </summary>
        /// <typeparam name="TPage"></typeparam>
        public void NavigateToContent<TPage>()
        {
            this.contentFrame?.Navigate(typeof(TPage));
        }

        /// <summary>
        /// Goes back to the previous page in the navigation history of the main frame, if possible.
        /// </summary>
        public void GoBack()
        {
            if (this.CanGoBack())
            {
                this.frame?.GoBack();
            }
        }

        /// <summary>
        /// Checks if there is a previous page in the navigation history
        /// of the main frame that can be navigated back to.
        /// </summary>
        /// <returns></returns>
        public bool CanGoBack()
        {
            return this.frame?.CanGoBack ?? false;
        }
    }
}
