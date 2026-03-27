using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MovieApp.Core.Models;
using MovieApp.Ui.Controls;
using MovieApp.Ui.Navigation;
using MovieApp.Ui.ViewModels.Events;

namespace MovieApp.Ui.Views;

/// <summary>
/// Hosts the discovery-first home experience, including horizontal event sections
/// and launch points into the other requirement-driven feature areas.
/// </summary>
public sealed partial class HomePage : Page
{
    private bool _initialized;

    /// <summary>
    /// Gets the page view model that owns search, sort, and section grouping state.
    /// </summary>
    public HomeEventsViewModel ViewModel { get; }

    public HomePage()
    {
        ViewModel = new HomeEventsViewModel(App.EventRepository);
        NavigationCacheMode = NavigationCacheMode.Required;
        InitializeComponent();
        DataContext = ViewModel;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (!_initialized)
        {
            _initialized = true;

            if (App.AmbassadorRepository is not null && App.CurrentUserService?.CurrentUser is { } currentUser)
            {
                var existingCode = await App.AmbassadorRepository.GetReferralCodeAsync(currentUser.Id);
                if (string.IsNullOrEmpty(existingCode))
                {
                    var generator = new MovieApp.Core.Services.ReferralCodeGenerator();
                    var newCode = generator.Generate(currentUser.Username, currentUser.Id);
                    await App.AmbassadorRepository.CreateAmbassadorProfileAsync(currentUser.Id, newCode);
                }
            }
        }

        // Refresh discounts every navigation so badges stay current after
        // jackpot wins or redeems on other pages.
        await EventCard.RefreshDiscountsAsync();
        await EventCard.RefreshJoinedEventIdsAsync();
        await ViewModel.InitializeAsync();
    }

    private void ShortcutButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.DataContext is not HomeNavigationShortcut shortcut)
        {
            return;
        }

        if (App.CurrentMainWindow is not null)
        {
            App.CurrentMainWindow.NavigateToRoute(shortcut.RouteTag);
            return;
        }

        Frame.Navigate(AppRouteResolver.ResolvePageType(shortcut.RouteTag));
    }

    /// <summary>
    /// Applies the current search text to the home page event list only.
    /// </summary>
    private void SearchBox_SearchTextChanged(object? sender, string searchText)
    {
        ViewModel.SetSearchText(searchText);
    }

    /// <summary>
    /// Applies the selected sort mode to the home page event list only.
    /// </summary>
    private void SortSelector_SortOptionChanged(object? sender, MovieApp.Core.EventLists.EventSortOption sortOption)
    {
        ViewModel.SetSortOption(sortOption);
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
        };

        var content = BuildEventDialogContent(selectedEvent, dialog);

        if (App.AmbassadorRepository is not null && App.CurrentUserService?.CurrentUser is { } currentUser)
        {
            int balance = await App.AmbassadorRepository.GetRewardBalanceAsync(currentUser.Id);
            if (balance > 0 && content is StackPanel layout)
            {
                var freePassButton = new Button
                {
                    Content = $"Use Free Pass ({balance} left)",
                    HorizontalAlignment = HorizontalAlignment.Left,
                };

                freePassButton.Click += async (_, _) =>
                {
                    var confirmDialog = new ContentDialog
                    {
                        XamlRoot = XamlRoot,
                        Title = "Use Free Pass?",
                        Content = "This will use 1 free enrollment credit. Continue?",
                        PrimaryButtonText = "Yes, enroll for free",
                        CloseButtonText = "Cancel",
                        DefaultButton = ContentDialogButton.Primary,
                    };

                    var result = await confirmDialog.ShowAsync();
                    if (result == ContentDialogResult.Primary)
                    {
                        await App.AmbassadorRepository.DecrementRewardBalanceAsync(currentUser.Id);
                        freePassButton.Content = "Free Pass applied";
                        freePassButton.IsEnabled = false;
                    }
                };

                layout.Children.Add(freePassButton);
            }
        }

        dialog.Content = content;
        await dialog.ShowAsync();
    }

    private static UIElement BuildEventDialogContent(Event selectedEvent, ContentDialog parentDialog)
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
            Text = $"Price: {EventCard.GetPriceText(selectedEvent, System.Globalization.CultureInfo.CurrentCulture)}",
        });

        // Show discounted price if user has a jackpot discount for this event
        if (EventCard.DiscountByEventId.TryGetValue(selectedEvent.Id, out var discountPct) && discountPct > 0 && selectedEvent.TicketPrice > 0)
        {
            layout.Children.Add(new Border
            {
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(0x33, 0xFF, 0xC1, 0x07)),
                CornerRadius = new Microsoft.UI.Xaml.CornerRadius(6),
                Padding = new Microsoft.UI.Xaml.Thickness(12, 8, 12, 8),
                Child = new TextBlock
                {
                    Text = $"🎰 Your price: {EventCard.GetDiscountedPriceText(selectedEvent, System.Globalization.CultureInfo.CurrentCulture, discountPct)}",
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                },
            });
        }

        layout.Children.Add(new TextBlock
        {
            Text = $"Rating: {EventCard.GetRatingText(selectedEvent)}",
        });

        layout.Children.Add(new TextBlock
        {
            Text = $"Seats: {EventCard.GetCapacityText(selectedEvent)}",
        });

        var referralTextBox = new TextBox
        {
            PlaceholderText = "Optional referral code",
            Width = 200,
        };
        var validationButton = new Button
        {
            Content = new FontIcon
            {
                Glyph = "\uE73E",
                FontSize = 14,
            },
        };

        validationButton.Click += async (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(referralTextBox.Text))
            {
                return;
            }

            if (App.ReferralValidator is not null && App.CurrentUserService?.CurrentUser is { } currentUser)
            {
                bool isValid = await App.ReferralValidator.IsValidReferralAsync(referralTextBox.Text, currentUser.Id);
                referralTextBox.BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    isValid ? Microsoft.UI.Colors.Green : Microsoft.UI.Colors.Red);
                referralTextBox.BorderThickness = new Microsoft.UI.Xaml.Thickness(2);
            }
        };

        layout.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children = { referralTextBox, validationButton },
        });

var willAttendBtn = new Button { Content = "Will attend", Tag = "Joined!" };
var buyTicketBtn = new Button { Content = "Buy ticket", Tag = "Ticket purchased!" };
EventCard.AttachJoinEventHandler(willAttendBtn, selectedEvent.Id);
EventCard.AttachJoinEventHandler(buyTicketBtn, selectedEvent.Id);

var seatGuideButton = new Button
{
    Content = "Seat guide",
};
seatGuideButton.Click += async (_, _) =>
{
    parentDialog.Hide();

    int capacity = selectedEvent.MaxCapacity > 0 ? selectedEvent.MaxCapacity : 50;
    var seatDialog = new SeatGuideDialog(capacity)
    {
        XamlRoot = parentDialog.XamlRoot,
    };
    await seatDialog.ShowAsync();
};

        layout.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                willAttendBtn,
                buyTicketBtn,
                new Button { Content = "Favorite" },
                seatGuideButton,
            },
        });

        return layout;
    }
}
