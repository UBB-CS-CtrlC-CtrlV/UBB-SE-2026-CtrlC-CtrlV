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

        /// <inheritdoc />
        public void SetFrame(Frame frame)
        {
            this.frame = frame;
        }

        /// <inheritdoc />
        public void SetContentFrame(Frame frame)
        {
            this.contentFrame = frame;
        }

        /// <inheritdoc />
        public void NavigateTo<TPage>()
        {
            this.frame?.Navigate(typeof(TPage));
        }

        /// <inheritdoc />
        public void NavigateToContent<TPage>()
        {
            this.contentFrame?.Navigate(typeof(TPage));
        }

        /// <inheritdoc />
        public void GoBack()
        {
            if (this.CanGoBack())
            {
                this.frame?.GoBack();
            }
        }

        /// <inheritdoc />
        public bool CanGoBack()
        {
            return this.frame?.CanGoBack ?? false;
        }
    }
}
