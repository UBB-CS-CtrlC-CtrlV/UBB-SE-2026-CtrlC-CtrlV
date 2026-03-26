using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MovieApp.Core.Models;
using MovieApp.Core.Models.Movie;
using MovieApp.Core.Repositories;
using MovieApp.Core.Services;
using Xunit;

namespace MovieApp.Core.Tests;

public sealed class SlotMachineServiceTests
{
    private sealed class InMemoryStateRepo : IUserSlotMachineStateRepository
    {
        private readonly Dictionary<int, UserSpinData> _store = new();

        public Task<UserSpinData?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            _store.TryGetValue(userId, out var data);
            return Task.FromResult(data);
        }

        public Task CreateAsync(UserSpinData state, CancellationToken cancellationToken = default)
        {
            _store[state.UserId] = state;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(UserSpinData state, CancellationToken cancellationToken = default)
        {
            _store[state.UserId] = state;
            return Task.CompletedTask;
        }
    }

    private sealed class InMemoryMovieRepo : IMovieRepository
    {
        public Task<IReadOnlyList<Genre>> GetGenresAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult((IReadOnlyList<Genre>)new List<Genre> { new Genre { Id = 1, Name = "Action" } });
        }

        public Task<IReadOnlyList<Actor>> GetActorsAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult((IReadOnlyList<Actor>)new List<Actor> { new Actor { Id = 2, Name = "Actor A" } });
        }

        public Task<IReadOnlyList<Director>> GetDirectorsAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult((IReadOnlyList<Director>)new List<Director> { new Director { Id = 3, Name = "Director A" } });
        }

        public Task<IReadOnlyList<Movie>> FindMoviesByCriteriaAsync(int genreId, int actorId, int directorId, CancellationToken cancellationToken = default)
        {
            var movie = new Movie { Id = 10, Title = "Action Hit", Description = "", ReleaseYear = 2020, DurationMinutes = 120 };
            return Task.FromResult((IReadOnlyList<Movie>)new List<Movie> { movie });
        }

        public Task<IReadOnlyList<int>> FindScreeningEventIdsForMovieAsync(int movieId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult((IReadOnlyList<int>)new List<int> { 100 });
        }
    }

    private sealed class InMemoryEventRepo : IEventRepository
    {
        public Task<IEnumerable<Event>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var future = DateTime.UtcNow.AddDays(2);
            return Task.FromResult((IEnumerable<Event>)new List<Event>
            {
                new Event { Id = 100, Title = "Action Hit Premiere", EventDateTime = future, PosterUrl = "", LocationReference = "Hall 1", TicketPrice = 15m, EventType = "Premiere", HistoricalRating = 4.5, CreatorUserId = 1 }
            });
        }

        public Task<IEnumerable<Event>> GetAllByTypeAsync(string eventType, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Event?> FindByIdAsync(int id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<int> AddAsync(Event @event, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> UpdateEnrollmentAsync(int eventId, int newCount, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class InMemoryDiscountRepo : IUserMovieDiscountRepository
    {
        public readonly List<Reward> Rewards = new();

        public Task AddAsync(Reward reward, CancellationToken cancellationToken = default)
        {
            Rewards.Add(reward);
            return Task.CompletedTask;
        }

        public Task<List<Reward>> GetDiscountsForUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Rewards.Where(r => r.OwnerUserId == userId).ToList());
        }
    }

    [Fact]
    public async Task SpinAsync_ConsumesDailySpinAndGrantsRewardForJackpot()
    {
        var stateRepo = new InMemoryStateRepo();
        var movieRepo = new InMemoryMovieRepo();
        var eventRepo = new InMemoryEventRepo();
        var discountRepo = new InMemoryDiscountRepo();

        var initialState = new UserSpinData { UserId = 1, DailySpinsRemaining = 1, BonusSpins = 0, LoginStreak = 3, EventSpinRewardsToday = 0 };
        await stateRepo.CreateAsync(initialState);

        var service = new SlotMachineService(stateRepo, movieRepo, eventRepo, discountRepo);

        var result = await service.SpinAsync(1);

        Assert.NotNull(result);
        Assert.Equal(0, (await stateRepo.GetByUserIdAsync(1))!.DailySpinsRemaining);
        Assert.True(result.JackpotDiscountApplied);
        Assert.Equal(1, discountRepo.Rewards.Count);
    }

    [Fact]
    public async Task SpinAsync_ThrowsWhenNoSpins()
    {
        var stateRepo = new InMemoryStateRepo();
        var movieRepo = new InMemoryMovieRepo();
        var eventRepo = new InMemoryEventRepo();
        var discountRepo = new InMemoryDiscountRepo();

        var initialState = new UserSpinData { UserId = 2, DailySpinsRemaining = 0, BonusSpins = 0, LoginStreak = 0, EventSpinRewardsToday = 0 };
        await stateRepo.CreateAsync(initialState);

        var service = new SlotMachineService(stateRepo, movieRepo, eventRepo, discountRepo);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.SpinAsync(2));
    }
}
