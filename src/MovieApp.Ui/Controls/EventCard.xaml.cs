using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using MovieApp.Core.Models;
using Windows.UI;

[assembly: InternalsVisibleTo("MovieApp.Ui.Tests")]

namespace MovieApp.Ui.Controls;

/// <summary>
/// Displays a compact event summary card and a seat-guide entry point for a single event.
/// </summary>
public sealed partial class EventCard : UserControl
{
    /// <summary>
    /// Maps event IDs to their available (non-redeemed) discount percentage.
    /// Populated via <see cref="RefreshDiscountsAsync"/> before EventCards are displayed.
    /// </summary>
    internal static Dictionary<int, int> DiscountByEventId { get; set; } = [];

    /// <summary>
    /// Reloads <see cref="DiscountByEventId"/> from the current user's non-redeemed
    /// movie discounts joined with screenings. Safe to call from any page.
    /// </summary>
    public static async Task RefreshDiscountsAsync()
    {
        if (App.CurrentUserService?.CurrentUser is not { } user ||
            App.UserMovieDiscountRepository is null ||
            App.ScreeningRepository is null)
        {
            DiscountByEventId = [];
            return;
        }

        var discounts = await App.UserMovieDiscountRepository.GetDiscountsForUserAsync(user.Id);

        var bestDiscountByMovie = discounts
            .Where(r => !r.RedemptionStatus && r.EventId.HasValue)
            .GroupBy(r => r.EventId!.Value)
            .ToDictionary(g => g.Key, g => (int)g.Max(r => r.DiscountValue));

        var result = new Dictionary<int, int>();
        foreach (var (movieId, discountPct) in bestDiscountByMovie)
        {
            var screenings = await App.ScreeningRepository.GetByMovieIdAsync(movieId);
            foreach (var screening in screenings)
            {
                if (!result.TryGetValue(screening.EventId, out var existing) || discountPct > existing)
                    result[screening.EventId] = discountPct;
            }
        }

        DiscountByEventId = result;
    }

    /// <summary>
    /// Event IDs the current user has already joined (loaded via <see cref="RefreshJoinedEventIdsAsync"/>).
    /// </summary>
    internal static HashSet<int> JoinedEventIds { get; private set; } = [];

    /// <summary>
    /// Reloads <see cref="JoinedEventIds"/> from the database for the current user.
    /// Safe to call on every navigation alongside <see cref="RefreshDiscountsAsync"/>.
    /// </summary>
    public static async Task RefreshJoinedEventIdsAsync()
    {
        if (App.UserEventAttendanceRepository is null ||
            App.CurrentUserService?.CurrentUser is not { } user)
        {
            JoinedEventIds = [];
            return;
        }

        var ids = await App.UserEventAttendanceRepository.GetJoinedEventIdsAsync(user.Id);
        JoinedEventIds = [..ids];
    }

    /// <summary>
    /// Configures <paramref name="button"/> for event-join behaviour:
    /// shows a locked "Already joined" state if the user has previously joined
    /// <paramref name="eventId"/>, otherwise attaches a handler that persists the
    /// join, grants a bonus spin (SM.28/SM.29), and updates the label immediately (SM.30).
    /// </summary>
    public static void AttachJoinEventHandler(Button button, int eventId)
    {
        if (JoinedEventIds.Contains(eventId))
        {
            button.Content = "✅ Already joined";
            button.IsEnabled = false;
            return;
        }

        button.Click += async (sender, _) =>
        {
            if (sender is not Button btn) return;

            btn.IsEnabled = false;

            if (App.CurrentUserService?.CurrentUser is { } user)
            {
                // Persist immediately so re-opened dialogs also see the locked state
                JoinedEventIds.Add(eventId);

                if (App.UserEventAttendanceRepository is not null)
                    await App.UserEventAttendanceRepository.JoinAsync(user.Id, eventId);

                if (App.SlotMachineService is not null)
                {
                    var granted = await App.SlotMachineService
                        .GrantBonusSpinForEventParticipationAsync(user.Id);

                    btn.Content = granted
                        ? $"✅ {btn.Tag} +1 bonus spin 🎰"
                        : $"✅ {btn.Tag}";
                }
                else
                {
                    btn.Content = $"✅ {btn.Tag}";
                }
            }
            else
            {
                btn.Content = $"✅ {btn.Tag}";
            }
        };
    }

    public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(
        nameof(Model),
        typeof(object),
        typeof(EventCard),
        new PropertyMetadata(null, OnEventChanged));

    public EventCard()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Gets or sets the event model rendered by the card.
    /// </summary>
    public object? Model
    {
        get => GetValue(ModelProperty);
        set => SetValue(ModelProperty, value);
    }

    private Event? EventModel => Model as Event;

    public string TitleText => GetTitleText(EventModel);
    public string DescriptionText => GetDescriptionText(EventModel);
    public string EventTypeText => GetEventTypeText(EventModel);
    public string DateBadgeDay => GetDateBadgeDay(EventModel, CultureInfo.CurrentCulture);
    public string ScheduleText => GetScheduleText(EventModel, CultureInfo.CurrentCulture);
    public string LocationText => GetLocationText(EventModel);

    public string PriceText => GetDiscountedPriceText(EventModel, CultureInfo.CurrentCulture);

    public string RatingText => GetRatingText(EventModel);
    public string CapacityText => GetCapacityText(EventModel);
    public string StatusText => GetStatusText(EventModel, DateTime.Now);
    public Brush StatusBackgroundBrush => new SolidColorBrush(GetStatusColor(EventModel, DateTime.Now));

    public bool HasDiscount => EventModel is not null && DiscountByEventId.ContainsKey(EventModel.Id);

    public Visibility DiscountBadgeVisibility => HasDiscount ? Visibility.Visible : Visibility.Collapsed;

    public string DiscountBadgeText => EventModel is not null && DiscountByEventId.TryGetValue(EventModel.Id, out var pct)
        ? $"🎰 -{pct}%"
        : "🎰 Discount";

    internal static string GetTitleText(Event? @event) => @event?.Title ?? "Untitled event";

    internal static string GetDescriptionText(Event? @event) => string.IsNullOrWhiteSpace(@event?.Description)
        ? "A curated movie experience with limited seating."
        : @event.Description!;

    internal static string GetEventTypeText(Event? @event) => string.IsNullOrWhiteSpace(@event?.EventType)
        ? "Special Event"
        : @event.EventType.Trim();

    internal static string GetDateBadgeDay(Event? @event, CultureInfo culture) => @event is null
        ? "--"
        : @event.EventDateTime.ToString("dd", culture);

    internal static string GetScheduleText(Event? @event, CultureInfo culture) => @event is null
        ? "Schedule to be announced"
        : @event.EventDateTime.ToString("ddd, MMM d • h:mm tt", culture);

    internal static string GetLocationText(Event? @event) => string.IsNullOrWhiteSpace(@event?.LocationReference)
        ? "Location to be announced"
        : @event.LocationReference;

    internal static string GetPriceText(Event? @event, CultureInfo culture) => @event is null
        ? "-"
        : @event.TicketPrice <= 0
            ? "Free"
            : @event.TicketPrice.ToString("C", culture);

    internal static string GetDiscountedPriceText(Event? @event, CultureInfo culture, int discountPercent)
    {
        if (@event is null) return "-";
        if (@event.TicketPrice <= 0) return "Free";
        if (discountPercent <= 0) return @event.TicketPrice.ToString("C", culture);
        var discounted = @event.TicketPrice * (1 - discountPercent / 100m);
        return $"{discounted.ToString("C", culture)} (-{discountPercent}%)";
    }

    private string GetDiscountedPriceText(Event? @event, CultureInfo culture)
    {
        if (@event is not null && DiscountByEventId.TryGetValue(@event.Id, out var pct) && pct > 0)
            return GetDiscountedPriceText(@event, culture, pct);
        return GetPriceText(@event, culture);
    }

    internal static string GetRatingText(Event? @event) => @event is null
        ? "-"
        : @event.HistoricalRating <= 0
            ? "New"
            : $"{@event.HistoricalRating:0.0}/5";

    internal static string GetCapacityText(Event? @event) => @event is null
        ? "-"
        : $"{@event.CurrentEnrollment}/{@event.MaxCapacity}";

    internal static string GetStatusText(Event? @event, DateTime now)
    {
        if (@event is null)
        {
            return "Pending";
        }

        if (@event.EventDateTime <= now)
        {
            return "Ended";
        }

        if (@event.AvailableSpots <= 0)
        {
            return "Sold out";
        }

        return @event.AvailableSpots == 1
            ? "1 spot left"
            : $"{@event.AvailableSpots} spots left";
    }

    internal static Color GetStatusColor(Event? @event, DateTime now)
    {
        if (@event is null)
        {
            return Color.FromArgb(0x1A, 0x80, 0x80, 0x80);
        }

        if (@event.EventDateTime <= now)
        {
            return Color.FromArgb(0x22, 0x94, 0x94, 0x94);
        }

        if (@event.AvailableSpots <= 0)
        {
            return Color.FromArgb(0x33, 0xD1, 0x34, 0x38);
        }

        return Color.FromArgb(0x33, 0x16, 0xA3, 0x4A);
    }

    private static void OnEventChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
    {
        if (dependencyObject is EventCard card)
        {
            card.RefreshComputedProperties();
        }
    }

    private async void WatcherButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (EventModel == null) return;

            var button = (Microsoft.UI.Xaml.Controls.Primitives.ToggleButton)sender;
            var isWatching = button.IsChecked ?? false;

            var folderPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "MovieApp");
            System.IO.Directory.CreateDirectory(folderPath);
            
            var repo = new MovieApp.Infrastructure.LocalPriceWatcherRepository(folderPath);

            if (isWatching)
            {
                var inputTextBox = new TextBox
                {
                    PlaceholderText = "Ex: 50.00",
                    Width = 200
                };

                var dialog = new ContentDialog
                {
                    Title = "Set Target Price",
                    Content = new StackPanel
                    {
                        Spacing = 10,
                        Children =
                        {
                            new TextBlock { Text = "Enter desired target price:" },
                            inputTextBox
                        }
                    },
                    PrimaryButtonText = "Save",
                    CloseButtonText = "Cancel",
                    XamlRoot = this.XamlRoot
                };

                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    string cleanInput = inputTextBox.Text.Replace(",", ".");
                    
                    if (decimal.TryParse(cleanInput, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal targetPrice))
                    {
                        var success = await repo.AddWatchAsync(new WatchedEvent { EventId = EventModel.Id, EventTitle = EventModel.Title, TargetPrice = targetPrice });
                        if (!success)
                        {
                            button.IsChecked = false;
                            var errorDialog = new ContentDialog
                            {
                                Title = "Limit Reached",
                                Content = "You can only watch up to 10 events, or you already watch this one.",
                                CloseButtonText = "OK",
                                XamlRoot = this.XamlRoot
                            };
                            await errorDialog.ShowAsync();
                        }
                    }
                    else
                    {
                        button.IsChecked = false;
                    }
                }
                else
                {
                    button.IsChecked = false;
                }
            }
            else
            {
                await repo.RemoveWatchAsync(EventModel.Id);
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Eroare la click: {ex.Message}");
        }
    }

    private async System.Threading.Tasks.Task SyncWatcherStateAsync()
    {
        if (EventModel == null || WatcherButton == null) return;
        
        var folderPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "MovieApp");
        System.IO.Directory.CreateDirectory(folderPath);
        
        var repo = new MovieApp.Infrastructure.LocalPriceWatcherRepository(folderPath);
        WatcherButton.IsChecked = await repo.IsWatchingAsync(EventModel.Id);
    }

    /// <summary>
    /// Recomputes the generated bindings when a new event model is assigned.
    /// </summary>
    private void RefreshComputedProperties()
    {
        Bindings.Update();
        _ = SyncWatcherStateAsync();
    }
}