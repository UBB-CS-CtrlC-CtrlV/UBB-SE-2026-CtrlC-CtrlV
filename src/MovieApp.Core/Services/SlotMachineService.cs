using MovieApp.Core.Models;
using MovieApp.Core.Models.Movie;
using MovieApp.Core.Repositories;

namespace MovieApp.Core.Services;

public sealed class SlotMachineService
{
    private readonly IUserSlotMachineStateRepository _stateRepository;
    private readonly IMovieRepository _movieRepository;
    private readonly IEventRepository _eventRepository;
    private readonly IUserMovieDiscountRepository _discountRepository;
    private readonly Random _random = new();

    public SlotMachineService(
        IUserSlotMachineStateRepository stateRepository,
        IMovieRepository movieRepository,
        IEventRepository eventRepository,
        IUserMovieDiscountRepository discountRepository)
    {
        _stateRepository = stateRepository;
        _movieRepository = movieRepository;
        _eventRepository = eventRepository;
        _discountRepository = discountRepository;
    }

    public async Task<SlotMachineResult> SpinAsync(int userId)
    {
        var state = await _stateRepository.GetByUserIdAsync(userId) ?? throw new InvalidOperationException("User state not found");

        var today = DateTime.UtcNow.Date;
        var lastReset = state.LastSlotSpinReset.Date;
        if (lastReset < today)
        {
            state.ResetDailySpins(5);
        }

        var totalSpins = state.DailySpinsRemaining + state.BonusSpins;
        if (totalSpins <= 0)
            throw new InvalidOperationException("No available spins");

        // consume spin
        if (state.DailySpinsRemaining > 0)
            state.DailySpinsRemaining--;
        else
            state.BonusSpins--;

        // generate reels
        var genre = await GetRandomGenreAsync();
        var actor = await GetRandomActorAsync();
        var director = await GetRandomDirectorAsync();

        // Match current reel results back to events and jackpot-eligible movies.
        var matchingEvents = await GetMatchingEventsAsync(genre.Id, actor.Id, director.Id);
        var jackpotMovie = await FindJackpotMovieAsync(genre.Id, actor.Id, director.Id);

        var result = new SlotMachineResult
        {
            Genre = genre,
            Actor = actor,
            Director = director,
            MatchingEvents = matchingEvents.ToList(),
            JackpotMovie = jackpotMovie is null ? null : jackpotMovie,
            JackpotDiscountApplied = false,
            DiscountPercentage = 0
        };

        if (jackpotMovie is not null)
        {
            await GrantJackpotDiscount(userId, jackpotMovie.Id);
            result.JackpotDiscountApplied = true;
            result.DiscountPercentage = 10;
        }

        await _stateRepository.UpdateAsync(state);

        return result;
    }

    public async Task<int> GetAvailableSpinsAsync(int userId)
    {
        var state = await _stateRepository.GetByUserIdAsync(userId) ?? throw new InvalidOperationException("User state not found");

        // Availability reads share the same reset rule as actual spins.
        var today = DateTime.UtcNow.Date;
        var lastReset = state.LastSlotSpinReset.Date;
        if (lastReset < today)
        {
            state.ResetDailySpins(5);
            await _stateRepository.UpdateAsync(state);
        }

        return state.DailySpinsRemaining + state.BonusSpins;
    }

    public async Task<bool> GrantBonusSpinForEventParticipationAsync(int userId)
    {
        var state = await _stateRepository.GetByUserIdAsync(userId) ?? throw new InvalidOperationException("User state not found");
        if (state.EventSpinRewardsToday < 2)
        {
            state.BonusSpins++;
            state.EventSpinRewardsToday++;
            await _stateRepository.UpdateAsync(state);
            return true;
        }
        return false;
    }

    public async Task<bool> GrantStreakSpinAsync(int userId)
    {
        var state = await _stateRepository.GetByUserIdAsync(userId) ?? throw new InvalidOperationException("User state not found");
        if (state.LoginStreak >= 3)
        {
            state.BonusSpins++;
            state.LoginStreak = 0;
            await _stateRepository.UpdateAsync(state);
            return true;
        }
        return false;
    }

    // Helper methods expose the reference data needed by the UI animation layer.
    public async Task<Genre> GetRandomGenreAsync(CancellationToken cancellationToken = default)
    {
        var genres = await _movieRepository.GetGenresAsync(cancellationToken);
        return genres[_random.Next(genres.Count)];
    }

    public async Task<Actor> GetRandomActorAsync(CancellationToken cancellationToken = default)
    {
        var actors = await _movieRepository.GetActorsAsync(cancellationToken);
        return actors[_random.Next(actors.Count)];
    }

    public async Task<Director> GetRandomDirectorAsync(CancellationToken cancellationToken = default)
    {
        var directors = await _movieRepository.GetDirectorsAsync(cancellationToken);
        return directors[_random.Next(directors.Count)];
    }

    public async Task<IReadOnlyList<Genre>> GetGenresAsync(CancellationToken cancellationToken = default)
    {
        return await _movieRepository.GetGenresAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Actor>> GetActorsAsync(CancellationToken cancellationToken = default)
    {
        return await _movieRepository.GetActorsAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Director>> GetDirectorsAsync(CancellationToken cancellationToken = default)
    {
        return await _movieRepository.GetDirectorsAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Event>> GetMatchingEventsAsync(int genreId, int actorId, int directorId)
    {
        var movies = await _movieRepository.FindMoviesByCriteriaAsync(genreId, actorId, directorId);
        var events = new List<Event>();
        foreach (var m in movies)
        {
            // use screenings table to map movies to events when available
            var eventIds = await _movieRepository.FindScreeningEventIdsForMovieAsync(m.Id);
            var allEvents = await _eventRepository.GetAllAsync();
            events.AddRange(allEvents.Where(e => eventIds.Contains(e.Id) && e.EventDateTime > DateTime.UtcNow));
        }

        return events;
    }

    public async Task<Movie?> FindJackpotMovieAsync(int genreId, int actorId, int directorId)
    {
        var movies = await _movieRepository.FindMoviesByCriteriaAsync(genreId, actorId, directorId);
        return movies.FirstOrDefault();
    }

    public async Task GrantJackpotDiscount(int userId, int movieId)
    {
        var reward = new Reward
        {
            RewardId = 0,
            RewardType = "MovieDiscount",
            RedemptionStatus = false,
            ApplicabilityScope = $"Movie:{movieId}",
            DiscountValue = 10.0,
            OwnerUserId = userId,
            EventId = null
        };

        await _discountRepository.AddAsync(reward);
    }
}
