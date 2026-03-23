using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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

    private void RefreshComputedProperties()
    {
        Bindings.Update();
    }
}
