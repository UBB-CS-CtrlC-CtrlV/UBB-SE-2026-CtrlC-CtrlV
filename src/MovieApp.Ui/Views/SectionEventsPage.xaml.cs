using System.Globalization;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MovieApp.Core.Models;
using MovieApp.Ui.Controls;
using MovieApp.Ui.ViewModels.Events;

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

    /// <summary>
    /// Gets the view model driving the currently selected section page.
    /// </summary>
    public SectionEventsViewModel? ViewModel { get; private set; }

    public SectionEventsPage()
    {
        InitializeComponent();
    }

    /// <inheritdoc />
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

        ViewModel = new SectionEventsViewModel(App.EventRepository, context);
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

    /// <summary>
    /// Applies the reusable sort selector to the currently loaded section list only.
    /// </summary>
    private void SortSelector_SortOptionChanged(object? sender, MovieApp.Core.EventLists.EventSortOption sortOption)
    {
        ViewModel?.SetSortOption(sortOption);
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

        var willAttendBtn = new Button { Content = "Will attend", Tag = "Joined!" };
        var buyTicketBtn = new Button { Content = "Buy ticket", Tag = "Ticket purchased!" };
        EventCard.AttachJoinEventHandler(willAttendBtn, selectedEvent.Id);
        EventCard.AttachJoinEventHandler(buyTicketBtn, selectedEvent.Id);

        layout.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                willAttendBtn,
                buyTicketBtn,
                new Button { Content = "Favorite" },
                new Button { Content = "Seat guide" },
            },
        });

        return layout;
    }
}
