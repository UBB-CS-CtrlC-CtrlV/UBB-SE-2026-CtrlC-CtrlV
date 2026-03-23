using Microsoft.UI.Xaml.Controls;

namespace MovieApp.Ui.Views;

/// <summary>
/// Provides the event detail, seat-guide, and checkout layout that later feature work
/// can plug into without reshaping the purchase flow.
/// </summary>
public sealed partial class DetailsCheckoutPage : Page
{
    public DetailsCheckoutPage()
    {
        InitializeComponent();
    }
}
