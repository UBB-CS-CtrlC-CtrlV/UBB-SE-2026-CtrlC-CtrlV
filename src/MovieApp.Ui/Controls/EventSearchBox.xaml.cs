using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MovieApp.Ui.Controls;

/// <summary>
/// Reusable free-text search input for event-list screens.
/// </summary>
/// <remarks>
/// This control is UI-only. It exposes the current text and a text-changed event so
/// each screen can apply the shared event-list search behavior to its own view model.
/// </remarks>
public sealed partial class EventSearchBox : UserControl
{
    /// <summary>
    /// Raised whenever the search text changes through user interaction or property sync.
    /// </summary>
    public event EventHandler<string>? SearchTextChanged;

    public EventSearchBox()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Gets or sets the current search text shown by the control.
    /// </summary>
    public string SearchText
    {
        get => (string)GetValue(SearchTextProperty);
        set => SetValue(SearchTextProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="SearchText"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty SearchTextProperty =
        DependencyProperty.Register(
            nameof(SearchText),
            typeof(string),
            typeof(EventSearchBox),
            new PropertyMetadata(string.Empty, OnSearchTextChanged));

    /// <summary>
    /// Gets or sets the placeholder text displayed when no search query is entered.
    /// </summary>
    public string PlaceholderText
    {
        get => (string)GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="PlaceholderText"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty PlaceholderTextProperty =
        DependencyProperty.Register(
            nameof(PlaceholderText),
            typeof(string),
            typeof(EventSearchBox),
            new PropertyMetadata("Search events"));

    private static void OnSearchTextChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
    {
        if (dependencyObject is not EventSearchBox searchBox)
        {
            return;
        }

        var newValue = args.NewValue as string ?? string.Empty;
        if (searchBox.SearchTextBox.Text != newValue)
        {
            searchBox.SearchTextBox.Text = newValue;
        }
    }

    /// <summary>
    /// Synchronizes the current text into the dependency property and notifies the host page.
    /// </summary>
    private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var newValue = SearchTextBox.Text ?? string.Empty;
        if (!string.Equals(SearchText, newValue, StringComparison.Ordinal))
        {
            SearchText = newValue;
        }

        SearchTextChanged?.Invoke(this, newValue);
    }
}
