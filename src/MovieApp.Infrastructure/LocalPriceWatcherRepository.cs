using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Infrastructure;

public sealed class LocalPriceWatcherRepository : IPriceWatcherRepository
{
    private readonly string _filePath;
    private const int MaxWatchLimit = 10;

    public LocalPriceWatcherRepository(string folderPath)
    {
        _filePath = Path.Combine(folderPath, "watched_events.json");
    }

    public async Task<List<WatchedEvent>> GetAllWatchedEventsAsync()
    {
        if (!File.Exists(_filePath))
        {
            return new List<WatchedEvent>();
        }

        try
        {
            var json = await File.ReadAllTextAsync(_filePath);
            return JsonSerializer.Deserialize<List<WatchedEvent>>(json) ?? new List<WatchedEvent>();
        }
        catch
        {
            return new List<WatchedEvent>();
        }
    }

    public async Task<bool> AddWatchAsync(WatchedEvent watchedEvent)
    {
        var events = await GetAllWatchedEventsAsync();

        if (events.Any(e => e.EventId == watchedEvent.EventId))
        {
            return false;
        }

        if (events.Count >= MaxWatchLimit)
        {
            return false;
        }

        events.Add(watchedEvent);
        await SaveAllAsync(events);
        return true;
    }

    public async Task RemoveWatchAsync(int eventId)
    {
        var events = await GetAllWatchedEventsAsync();
        var itemToRemove = events.FirstOrDefault(e => e.EventId == eventId);
        
        if (itemToRemove != null)
        {
            events.Remove(itemToRemove);
            await SaveAllAsync(events);
        }
    }

    public async Task<WatchedEvent?> GetWatchAsync(int eventId)
    {
        var events = await GetAllWatchedEventsAsync();
        return events.FirstOrDefault(e => e.EventId == eventId);
    }

    public async Task<bool> IsWatchingAsync(int eventId)
    {
        var events = await GetAllWatchedEventsAsync();
        return events.Any(e => e.EventId == eventId);
    }

    private async Task SaveAllAsync(List<WatchedEvent> events)
    {
        var json = JsonSerializer.Serialize(events);
        await File.WriteAllTextAsync(_filePath, json);
    }
}