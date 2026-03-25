using MovieApp.Core.Models;
using Xunit;

namespace MovieApp.Core.Tests;

public sealed class SeatTests
{
    [Fact]
    public void SeatColor_ReturnsRed_WhenQualityIsPoor()
    {
        var seat = new Seat { Quality = SeatQuality.Poor };

        Assert.Equal("#FF4D4D", seat.SeatColor);
    }

    [Fact]
    public void SeatColor_ReturnsGreen_WhenQualityIsOptimal()
    {
        var seat = new Seat { Quality = SeatQuality.Optimal };

        Assert.Equal("#4CAF50", seat.SeatColor);
    }

    [Fact]
    public void SeatColor_ReturnsYellow_WhenQualityIsStandard()
    {
        var seat = new Seat { Quality = SeatQuality.Standard };

        Assert.Equal("#FFC107", seat.SeatColor);
    }

    [Fact]
    public void IsAvailable_DefaultsToTrue()
    {
        var seat = new Seat();

        Assert.True(seat.IsAvailable);
    }
}