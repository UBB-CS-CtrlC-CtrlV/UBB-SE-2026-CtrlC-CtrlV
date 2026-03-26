using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MovieApp.Core.Models;
using Xunit;

namespace MovieApp.Infrastructure.Tests;

public sealed class LocalPriceWatcherRepositoryTests : IDisposable
{
    private readonly string _testFolderPath;
    private readonly LocalPriceWatcherRepository _repository;

    public LocalPriceWatcherRepositoryTests()
    {
        _testFolderPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testFolderPath);
        
        _repository = new LocalPriceWatcherRepository(_testFolderPath);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testFolderPath))
        {
            Directory.Delete(_testFolderPath, true);
        }
    }

    [Fact]
    public async Task GetAllWatchedEventsAsync_WhenFileDoesNotExist_ReturnsEmptyList()
    {
        var result = await _repository.GetAllWatchedEventsAsync();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task AddWatchAsync_WhenValidEvent_AddsEventAndReturnsTrue()
    {
        var newEvent = new WatchedEvent { EventId = 1 };

        var result = await _repository.AddWatchAsync(newEvent);
        var allEvents = await _repository.GetAllWatchedEventsAsync();

        Assert.True(result);
        Assert.Single(allEvents);
        Assert.Equal(1, allEvents.First().EventId);
    }

    [Fact]
    public async Task AddWatchAsync_WhenEventAlreadyExists_ReturnsFalse()
    {
        var existingEvent = new WatchedEvent { EventId = 1 };
        await _repository.AddWatchAsync(existingEvent);

        var duplicateEvent = new WatchedEvent { EventId = 1 };

        var result = await _repository.AddWatchAsync(duplicateEvent);

        Assert.False(result);
        var allEvents = await _repository.GetAllWatchedEventsAsync();
        Assert.Single(allEvents);
    }

    [Fact]
    public async Task AddWatchAsync_WhenMaxLimitReached_ReturnsFalse()
    {
        for (int i = 1; i <= 10; i++)
        {
            await _repository.AddWatchAsync(new WatchedEvent { EventId = i });
        }

        var eventOverLimit = new WatchedEvent { EventId = 11 };

        var result = await _repository.AddWatchAsync(eventOverLimit);

        Assert.False(result);
        var allEvents = await _repository.GetAllWatchedEventsAsync();
        Assert.Equal(10, allEvents.Count);
    }

    [Fact]
    public async Task RemoveWatchAsync_WhenEventExists_RemovesEvent()
    {
        await _repository.AddWatchAsync(new WatchedEvent { EventId = 1 });
        await _repository.AddWatchAsync(new WatchedEvent { EventId = 2 });

        await _repository.RemoveWatchAsync(1);

        var allEvents = await _repository.GetAllWatchedEventsAsync();
        Assert.Single(allEvents);
        Assert.Equal(2, allEvents.First().EventId);
    }
    
    [Fact]
    public async Task IsWatchingAsync_WhenEventIsWatched_ReturnsTrue()
    {
        await _repository.AddWatchAsync(new WatchedEvent { EventId = 5 });

        var result = await _repository.IsWatchingAsync(5);

        Assert.True(result);
    }
}