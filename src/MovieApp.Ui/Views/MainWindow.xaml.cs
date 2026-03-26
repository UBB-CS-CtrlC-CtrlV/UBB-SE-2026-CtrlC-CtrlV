using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MovieApp.Ui.Navigation;
using MovieApp.Ui.ViewModels;

namespace MovieApp.Ui.Views;

/// <summary>
/// Hosts the application shell, navigation structure, and the top-level frame
/// where each requirement-driven feature page is loaded.
/// </summary>
public sealed partial class MainWindow : Window
{
    /// <summary>
    /// Creates the main application shell and loads the default home route.
    /// </summary>
    public MainWindow(MainViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();

        NavigateToRoute(AppRouteResolver.Home);
    }

    /// <summary>
    /// Gets the shell view model describing the current user or startup state.
    /// </summary>
    public MainViewModel ViewModel { get; }

    /// <summary>
    /// Navigates the shell frame to the page associated with the given route tag.
    /// </summary>
    /// <param name="tag">The route selected in the shell.</param>
    public void NavigateToRoute(string tag)
    {
        var pageType = AppRouteResolver.ResolvePageType(tag);
        SyncSelectedNavigationItem(tag);

        if (ContentFrame.CurrentSourcePageType != pageType)
        {
            ContentFrame.Navigate(pageType);
        }
    }

    private void AppNavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItemContainer?.Tag is string tag)
        {
            NavigateToRoute(tag);
        }
    }

    private void SyncSelectedNavigationItem(string tag)
    {
        var selectedItem = AppNavigationView.MenuItems
            .OfType<NavigationViewItem>()
            .Concat(AppNavigationView.FooterMenuItems.OfType<NavigationViewItem>())
            .FirstOrDefault(item => string.Equals(item.Tag as string, tag, StringComparison.Ordinal));

        AppNavigationView.SelectedItem = selectedItem;
    }
}
