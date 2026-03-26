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

        var totalSpins = state.DailySpinsRemaining + state.BonusSpins;
        if (totalSpins <= 0)
            throw new InvalidOperationException("No available spins");

        // consume spin
        if (state.DailySpinsRemaining > 0)
            state.DailySpinsRemaining--;
        else
            state.BonusSpins--;

        // generate reels
        var genre = GetRandomGenre();
        var actor = GetRandomActor();
        var director = GetRandomDirector();

        // find matching events and jackpot
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

    // Helpers: these use the movie and event repositories to select from active movies
    public Genre GetRandomGenre()
    {
        var genres = _movieRepository.GetGenresAsync().GetAwaiter().GetResult();
        return genres[_random.Next(genres.Count)];
    }

    public Actor GetRandomActor()
    {
        var actors = _movieRepository.GetActorsAsync().GetAwaiter().GetResult();
        return actors[_random.Next(actors.Count)];
    }

    public Director GetRandomDirector()
    {
        var directors = _movieRepository.GetDirectorsAsync().GetAwaiter().GetResult();
        return directors[_random.Next(directors.Count)];
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
