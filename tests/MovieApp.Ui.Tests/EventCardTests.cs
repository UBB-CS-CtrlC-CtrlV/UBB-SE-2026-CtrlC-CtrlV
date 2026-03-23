using System.Globalization;
using MovieApp.Core.Models;
using MovieApp.Ui.Controls;
using Windows.UI;
using Xunit;

namespace MovieApp.Ui.Tests;

public sealed class EventCardTests
{
    [Fact]
    public void FormattingHelpers_ReturnFallbacksForMissingEvent()
    {
        Assert.Equal("Untitled event", EventCard.GetTitleText(null));
        Assert.Equal("A curated movie experience with limited seating.", EventCard.GetDescriptionText(null));
        Assert.Equal("Special Event", EventCard.GetEventTypeText(null));
        Assert.Equal("--", EventCard.GetDateBadgeDay(null, CultureInfo.InvariantCulture));
        Assert.Equal("Schedule to be announced", EventCard.GetScheduleText(null, CultureInfo.InvariantCulture));
        Assert.Equal("Location to be announced", EventCard.GetLocationText(null));
        Assert.Equal("-", EventCard.GetPriceText(null, CultureInfo.InvariantCulture));
        Assert.Equal("-", EventCard.GetRatingText(null));
        Assert.Equal("-", EventCard.GetCapacityText(null));
        Assert.Equal("Pending", EventCard.GetStatusText(null, DateTime.UnixEpoch));
    }

    [Fact]
    public void FormattingHelpers_FormatEventDetailsForDisplay()
    {
        var @event = BuildEvent(
            eventDateTime: new DateTime(2030, 5, 8, 19, 30, 0),
            ticketPrice: 25,
            historicalRating: 4.7,
            maxCapacity: 80,
            currentEnrollment: 24,
            description: "One-night-only anniversary screening.",
            eventType: " Premiere ",
            locationReference: "Grand Hall");

        Assert.Equal("Galaxy Premiere", EventCard.GetTitleText(@event));
        Assert.Equal("One-night-only anniversary screening.", EventCard.GetDescriptionText(@event));
        Assert.Equal("Premiere", EventCard.GetEventTypeText(@event));
        Assert.Equal("08", EventCard.GetDateBadgeDay(@event, CultureInfo.InvariantCulture));
        Assert.Equal("Wed, May 8 • 7:30 PM", EventCard.GetScheduleText(@event, CultureInfo.InvariantCulture));
        Assert.Equal("Grand Hall", EventCard.GetLocationText(@event));
        Assert.Equal("$25.00", EventCard.GetPriceText(@event, CultureInfo.GetCultureInfo("en-US")));
        Assert.Equal("4.7/5", EventCard.GetRatingText(@event));
        Assert.Equal("24/80", EventCard.GetCapacityText(@event));
    }

    [Fact]
    public void StatusHelpers_ReturnOpenStateForAvailableEvent()
    {
        var now = new DateTime(2030, 5, 1, 12, 0, 0);
        var @event = BuildEvent(
            eventDateTime: now.AddDays(2),
            ticketPrice: 25,
            historicalRating: 4.7,
            maxCapacity: 80,
            currentEnrollment: 79);

        Assert.Equal("1 spot left", EventCard.GetStatusText(@event, now));
        Assert.Equal(Color.FromArgb(0x33, 0x16, 0xA3, 0x4A), EventCard.GetStatusColor(@event, now));
    }

    [Fact]
    public void StatusHelpers_ReturnSoldOutStateWhenNoSpotsRemain()
    {
        var now = new DateTime(2030, 5, 1, 12, 0, 0);
        var @event = BuildEvent(
            eventDateTime: now.AddDays(2),
            ticketPrice: 25,
            historicalRating: 4.7,
            maxCapacity: 80,
            currentEnrollment: 80);

        Assert.Equal("Sold out", EventCard.GetStatusText(@event, now));
        Assert.Equal(Color.FromArgb(0x33, 0xD1, 0x34, 0x38), EventCard.GetStatusColor(@event, now));
    }

    [Fact]
    public void StatusHelpers_ReturnEndedStateForPastEvent()
    {
        var now = new DateTime(2030, 5, 10, 12, 0, 0);
        var @event = BuildEvent(
            eventDateTime: now.AddHours(-2),
            ticketPrice: 25,
            historicalRating: 4.7,
            maxCapacity: 80,
            currentEnrollment: 40);

        Assert.Equal("Ended", EventCard.GetStatusText(@event, now));
        Assert.Equal(Color.FromArgb(0x22, 0x94, 0x94, 0x94), EventCard.GetStatusColor(@event, now));
    }

    private static Event BuildEvent(
        DateTime eventDateTime,
        decimal ticketPrice,
        double historicalRating,
        int maxCapacity,
        int currentEnrollment,
        string description = "",
        string eventType = "Festival",
        string locationReference = "Main Stage")
    {
        return new Event
        {
            Id = 42,
            Title = "Galaxy Premiere",
            Description = description,
            PosterUrl = string.Empty,
            EventDateTime = eventDateTime,
            LocationReference = locationReference,
            TicketPrice = ticketPrice,
            HistoricalRating = historicalRating,
            EventType = eventType,
            MaxCapacity = maxCapacity,
            CurrentEnrollment = currentEnrollment,
            CreatorUserId = 7,
        };
    }
}
