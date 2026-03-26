using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;
using MovieApp.Ui.Controls;
using MovieApp.Ui.Services;
using MovieApp.Ui.ViewModels.Events;
using System.Globalization;

namespace MovieApp.Ui.Views;

/// <summary>
/// Displays the events belonging to a single home-page section.
/// </summary>
/// <remarks>
/// Search and filtering stay scoped to the selected section because this page owns
/// its own <see cref="SectionEventsViewModel"/> instance.
/// </remarks>
public sealed partial class SectionEventsPage : Page
{
    private bool _initialized;

    public SectionEventsViewModel? ViewModel { get; private set; }

    private static IEventRepository GetEventRepository()
        => App.EventRepository ?? UnavailableEventRepository.Instance;

    public SectionEventsPage()
    {
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (_initialized)
        {
            return;
        }

        if (e.Parameter is not SectionNavigationContext context)
        {
            return;
        }

        ViewModel = new SectionEventsViewModel(GetEventRepository(), context);
        DataContext = ViewModel;

        _initialized = true;
        await ViewModel.InitializeAsync();
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (Frame.CanGoBack)
        {
            Frame.GoBack();
        }
    }

    /// <summary>
    /// Applies the reusable search input to the currently loaded section list only.
    /// </summary>
    private void SearchBox_SearchTextChanged(object? sender, string searchText)
    {
        ViewModel?.SetSearchText(searchText);
    }

    private async void EventCardButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.DataContext is not Event selectedEvent)
        {
            return;
        }

        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Title = selectedEvent.Title,
            PrimaryButtonText = "Close",
            DefaultButton = ContentDialogButton.Primary,
            Content = BuildEventDialogContent(selectedEvent),
        };

        await dialog.ShowAsync();
    }

    private static UIElement BuildEventDialogContent(Event selectedEvent)
    {
        var layout = new StackPanel
        {
            Spacing = 12,
        };

        layout.Children.Add(new TextBlock
        {
            Text = selectedEvent.Description,
            TextWrapping = TextWrapping.WrapWholeWords,
        });

        layout.Children.Add(new TextBlock
        {
            Text = $"When: {selectedEvent.EventDateTime:g}",
        });

        layout.Children.Add(new TextBlock
        {
            Text = $"Where: {selectedEvent.LocationReference}",
        });

        layout.Children.Add(new TextBlock
        {
            Text = $"Price: {EventCard.GetPriceText(selectedEvent, CultureInfo.CurrentCulture)}",
        });

        layout.Children.Add(new TextBlock
        {
            Text = $"Rating: {EventCard.GetRatingText(selectedEvent)}",
        });

        layout.Children.Add(new TextBlock
        {
            Text = $"Seats: {EventCard.GetCapacityText(selectedEvent)}",
        });

        return layout;
    }
}
