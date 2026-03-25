using System.Linq;
using MovieApp.Core.Models;
using MovieApp.Ui.ViewModels.Events;
using Xunit;

namespace MovieApp.Ui.Tests;

public sealed class SeatGuideViewModelTests
{
    [Fact]
    public void Constructor_GivenCapacity_SetsTotalRowsAndColumnsCorrectly()
    {
        var viewModel = new SeatGuideViewModel(50);

        Assert.Equal(50, viewModel.Seats.Count);
        Assert.Equal(5, viewModel.TotalRows);
        Assert.Equal(10, viewModel.TotalColumns);
    }

    [Fact]
    public void Constructor_GivenCapacityNotDivisibleByTen_CreatesExactNumberOfSeats()
    {
        var viewModel = new SeatGuideViewModel(54);

        Assert.Equal(54, viewModel.Seats.Count);
        Assert.Equal(6, viewModel.TotalRows); 
    }

    [Fact]
    public void Constructor_SetsFirstTwoRowsToPoorQuality()
    {
        var viewModel = new SeatGuideViewModel(50);

        var frontSeats = viewModel.Seats.Where(s => s.Row <= 2).ToList();

        Assert.NotEmpty(frontSeats);
        Assert.All(frontSeats, s => Assert.Equal(SeatQuality.Poor, s.Quality));
        Assert.All(frontSeats, s => Assert.False(s.IsSweetSpot));
    }

    [Fact]
    public void Constructor_CalculatesSweetSpotAtTheCenter()
    {
        var viewModel = new SeatGuideViewModel(50);

        var sweetSpots = viewModel.Seats.Where(s => s.IsSweetSpot).ToList();

        Assert.NotEmpty(sweetSpots);
        Assert.All(sweetSpots, s => Assert.Equal(SeatQuality.Optimal, s.Quality));
        
        Assert.All(sweetSpots, s => Assert.True(s.Row is >= 3 and <= 4));
        Assert.All(sweetSpots, s => Assert.True(s.Column is >= 4 and <= 6));
    }

    [Fact]
    public void Constructor_SetsRandomAvailabilityForSeats()
    {

        var viewModel = new SeatGuideViewModel(200);

        var unavailableSeats = viewModel.Seats.Where(s => !s.IsAvailable).ToList();

        Assert.NotEmpty(unavailableSeats);
    }
}