using MovieApp.Core.Models;
using MovieApp.Ui.Services;
using Xunit;

namespace MovieApp.Ui.Tests;

public sealed class UnavailableEventRepositoryTests
{
    [Fact]
    public async Task ReadOperations_ReturnSafeEmptyResults()
    {
        var repository = UnavailableEventRepository.Instance;

        var allEvents = await repository.GetAllAsync();
        var filteredEvents = await repository.GetAllByTypeAsync("Festival");
        var eventById = await repository.FindByIdAsync(42);

        Assert.NotEmpty(allEvents);
        Assert.Empty(filteredEvents);
        Assert.Null(eventById);
    }

    [Fact]
    public async Task WriteOperations_ReturnSafeNoOpResults()
    {
        var repository = UnavailableEventRepository.Instance;
        var result = await repository.AddAsync(new Event
        {
            Id = 1,
            Title = "Placeholder",
            Description = "Fallback repository write",
            PosterUrl = string.Empty,
            EventDateTime = new DateTime(2030, 1, 1, 12, 0, 0),
            LocationReference = "Nowhere",
            TicketPrice = 0,
            HistoricalRating = 0,
            EventType = "Fallback",
            MaxCapacity = 1,
            CurrentEnrollment = 0,
            CreatorUserId = 1,
        });

        var updated = await repository.UpdateEnrollmentAsync(1, 1);

        Assert.Equal(1, result);
        Assert.True(updated);
    }
}
