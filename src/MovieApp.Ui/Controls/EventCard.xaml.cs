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

public sealed partial class EventCard : UserControl
{
    public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(
        nameof(Model),
        typeof(object),
        typeof(EventCard),
        new PropertyMetadata(null, OnEventChanged));

    public EventCard()
    {
        InitializeComponent();
    }

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

    public string PriceText => GetPriceText(EventModel, CultureInfo.CurrentCulture);

    public string RatingText => GetRatingText(EventModel);

    public string CapacityText => GetCapacityText(EventModel);

    public string StatusText => GetStatusText(EventModel, DateTime.Now);

    public Brush StatusBackgroundBrush => new SolidColorBrush(GetStatusColor(EventModel, DateTime.Now));

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
    
    private async void VisualSeatGuide_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        try
        {
            int capacity = EventModel?.MaxCapacity ?? 50;

            var dialog = new SeatGuideDialog(capacity)
            {
                XamlRoot = this.XamlRoot 
            };

            await dialog.ShowAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Eroare la deschiderea dialogului: {ex.Message}");
            System.Diagnostics.Debug.WriteLine(ex.StackTrace);
        }
    }

    private async void WatcherButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (EventModel == null) return;

            var button = (Microsoft.UI.Xaml.Controls.Primitives.ToggleButton)sender;
            var isWatching = button.IsChecked ?? false;

            // Folosim folderul sigur pentru Windows, care nu dă erori în WinUI 3 Unpackaged
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
                        button.IsChecked = false; // A introdus text invalid
                    }
                }
                else
                {
                    button.IsChecked = false; // A apasat Cancel
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
    private void RefreshComputedProperties()
    {
        Bindings.Update();
        _ = SyncWatcherStateAsync();
    }
}