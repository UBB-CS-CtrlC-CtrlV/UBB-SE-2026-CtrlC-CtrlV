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

        // Check if daily spins need to be reset (more than a day has passed)
        var today = DateTime.UtcNow.Date;
        var lastReset = state.LastSlotSpinReset.Date;
        if (lastReset < today)
        {
            state.ResetDailySpins(5); // Reset to 5 daily spins
        }

        var totalSpins = state.DailySpinsRemaining + state.BonusSpins;
        if (totalSpins <= 0)
            throw new InvalidOperationException("No available spins");

        // consume spin
        if (state.DailySpinsRemaining > 0)
            state.DailySpinsRemaining--;
        else
            state.BonusSpins--;

        // Pick each reel independently from screened value pools
        var validCombinations = await _movieRepository.GetValidReelCombinationsAsync();
        if (validCombinations.Count == 0)
            throw new InvalidOperationException("No movies with active screenings available");

        var distinctGenres = validCombinations.Select(c => c.Genre).DistinctBy(g => g.Id).ToList();
        var distinctActors = validCombinations.Select(c => c.Actor).DistinctBy(a => a.Id).ToList();
        var distinctDirectors = validCombinations.Select(c => c.Director).DistinctBy(d => d.Id).ToList();

        var genre = distinctGenres[_random.Next(distinctGenres.Count)];
        var actor = distinctActors[_random.Next(distinctActors.Count)];
        var director = distinctDirectors[_random.Next(distinctDirectors.Count)];

        // find matching events (OR logic) and jackpot (AND logic)
        var matchingEvents = await GetMatchingEventsAsync(genre.Id, actor.Id, director.Id);
        var jackpotMovie = await FindJackpotMovieAsync(genre.Id, actor.Id, director.Id);

        // Compute jackpot event IDs for UI highlighting
        var jackpotEventIds = new HashSet<int>();
        if (jackpotMovie is not null)
        {
            var jpEventIds = await _movieRepository.FindScreeningEventIdsForMovieAsync(jackpotMovie.Id);
            foreach (var id in jpEventIds)
                jackpotEventIds.Add(id);
        }

        var result = new SlotMachineResult
        {
            Genre = genre,
            Actor = actor,
            Director = director,
            MatchingEvents = matchingEvents.ToList(),
            JackpotEventIds = jackpotEventIds,
            JackpotMovie = jackpotMovie,
            JackpotDiscountApplied = false,
            DiscountPercentage = 0
        };

        if (jackpotMovie is not null)
        {
            await GrantJackpotDiscount(userId, jackpotMovie.Id);
            result.JackpotDiscountApplied = true;
            result.DiscountPercentage = 70;
        }

        await _stateRepository.UpdateAsync(state);

        return result;
    }

    public async Task<int> GetAvailableSpinsAsync(int userId)
    {
        var state = await _stateRepository.GetByUserIdAsync(userId) ?? throw new InvalidOperationException("User state not found");

        // Check if daily spins need to be reset (more than a day has passed)
        var today = DateTime.UtcNow.Date;
        var lastReset = state.LastSlotSpinReset.Date;
        if (lastReset < today)
        {
            state.ResetDailySpins(5); // Reset to 5 daily spins
            await _stateRepository.UpdateAsync(state); // Persist the reset
        }

        return state.DailySpinsRemaining + state.BonusSpins;
    }

    /// <summary>
    /// Returns the full spin state for a user (daily spins, bonus spins, login streak),
    /// resetting daily spins if the day has rolled over.
    /// </summary>
    public async Task<UserSpinData> GetUserSpinStateAsync(int userId)
    {
        var state = await _stateRepository.GetByUserIdAsync(userId) ?? throw new InvalidOperationException("User state not found");

        var today = DateTime.UtcNow.Date;
        if (state.LastSlotSpinReset.Date < today)
        {
            state.ResetDailySpins(5);
            await _stateRepository.UpdateAsync(state);
        }

        return state;
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

    /// <summary>
    /// Records the current login against the user's streak counter and, if a
    /// three-day consecutive streak is reached (SM.32), grants one bonus spin
    /// and resets the streak counter (SM.33).
    /// Safe to call once per app session; calling more than once on the same day
    /// is idempotent because <see cref="UserSpinData.UpdateLoginStreak"/> only
    /// increments when the last login was on the previous calendar day.
    /// </summary>
    /// <returns><c>true</c> if a streak bonus spin was awarded.</returns>
    public async Task<bool> RecordLoginAndCheckStreakAsync(int userId)
    {
        var state = await _stateRepository.GetByUserIdAsync(userId) ?? throw new InvalidOperationException("User state not found");

        state.UpdateLoginStreak();

        bool granted = false;
        if (state.LoginStreak >= 3)
        {
            state.BonusSpins++;
            state.LoginStreak = 0; // SM.33: reset after awarding
            granted = true;
        }

        await _stateRepository.UpdateAsync(state);
        return granted;
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

    // Helpers: these use the movie and event repositories to select from active movies
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
        var movies = await _movieRepository.FindMoviesByAnyCriteriaAsync(genreId, actorId, directorId);
        var events = new List<Event>();
        foreach (var m in movies)
        {
            var eventIds = await _movieRepository.FindScreeningEventIdsForMovieAsync(m.Id);
            var allEvents = await _eventRepository.GetAllAsync();
            events.AddRange(allEvents.Where(e => eventIds.Contains(e.Id) && e.EventDateTime > DateTime.UtcNow));
        }

        return events.DistinctBy(e => e.Id).ToList();
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
            DiscountValue = 70.0,
            OwnerUserId = userId,
            EventId = movieId
        };

        await _discountRepository.AddAsync(reward);
    }
}
