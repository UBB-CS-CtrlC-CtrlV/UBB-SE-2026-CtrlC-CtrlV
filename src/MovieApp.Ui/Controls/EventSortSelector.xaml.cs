using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MovieApp.Core.EventLists;

namespace MovieApp.Ui.Controls;

/// <summary>
/// Reusable single-select sort control for event-list screens.
/// </summary>
/// <remarks>
/// This control exposes the shared event sort modes used by all list screens and
/// ensures only one sort option is selected at a time through its combo-box UI.
/// </remarks>
public sealed partial class EventSortSelector : UserControl
{
    private static readonly IReadOnlyList<EventSortOptionItem> DefaultSortOptions =
    [
        new EventSortOptionItem(EventSortOption.DateAscending, "Date: soonest first"),
        new EventSortOptionItem(EventSortOption.DateDescending, "Date: latest first"),
        new EventSortOptionItem(EventSortOption.PriceAscending, "Price: low to high"),
        new EventSortOptionItem(EventSortOption.PriceDescending, "Price: high to low"),
        new EventSortOptionItem(EventSortOption.HistoricalRatingDescending, "Historical rating")
    ];

    private bool _isLoaded;
    private EventSortOption _pendingSortOption = EventSortOption.DateAscending;

    /// <summary>
    /// Raised when the user selects a different sort mode.
    /// </summary>
    public event EventHandler<EventSortOption>? SortOptionChanged;

    public EventSortSelector()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    /// <summary>
    /// Gets or sets the currently selected event sort option.
    /// </summary>
    public EventSortOption SelectedSortOption
    {
        get => (EventSortOption)GetValue(SelectedSortOptionProperty);
        set => SetValue(SelectedSortOptionProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="SelectedSortOption"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty SelectedSortOptionProperty =
        DependencyProperty.Register(
            nameof(SelectedSortOption),
            typeof(EventSortOption),
            typeof(EventSortSelector),
            new PropertyMetadata(EventSortOption.DateAscending, OnSelectedSortOptionChanged));

    /// <summary>
    /// Gets or sets the placeholder text shown by the selector before a value is chosen.
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
            typeof(EventSortSelector),
            new PropertyMetadata("Sort events"));

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _isLoaded = true;
        SortComboBox.ItemsSource = DefaultSortOptions;
        UpdateSelectedItem(_pendingSortOption);
    }

    private static void OnSelectedSortOptionChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
    {
        if (dependencyObject is not EventSortSelector selector || args.NewValue is not EventSortOption sortOption)
        {
            return;
        }

        selector._pendingSortOption = sortOption;
        selector.UpdateSelectedItem(sortOption);
    }

    /// <summary>
    /// Synchronizes the selected combo-box item with the dependency property value.
    /// </summary>
    private void UpdateSelectedItem(EventSortOption sortOption)
    {
        if (!_isLoaded || SortComboBox is null)
        {
            return;
        }

        var selectedItem = DefaultSortOptions.FirstOrDefault(item => item.Value == sortOption);
        if (!ReferenceEquals(SortComboBox.SelectedItem, selectedItem))
        {
            SortComboBox.SelectedItem = selectedItem;
        }
    }

    /// <summary>
    /// Propagates the chosen sort mode back to the host page.
    /// </summary>
    private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SortComboBox.SelectedItem is not EventSortOptionItem selectedItem)
        {
            return;
        }

        if (SelectedSortOption != selectedItem.Value)
        {
            SelectedSortOption = selectedItem.Value;
        }

        SortOptionChanged?.Invoke(this, selectedItem.Value);
    }

    /// <summary>
    /// View model for one reusable sort option entry in the selector.
    /// </summary>
    private sealed record EventSortOptionItem(EventSortOption Value, string Label);
}
