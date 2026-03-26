using MovieApp.Core.Models;
using MovieApp.Core.Repositories;
using MovieApp.Core.Services;
using System.Collections.ObjectModel;

namespace MovieApp.Ui.ViewModels;

public sealed class MarathonPageViewModel : ViewModelBase
{
    private readonly IMarathonService _marathonService;
    private readonly IMarathonRepository _marathonRepository;

    private Marathon? _selectedMarathon;
    private IReadOnlyList<LeaderboardEntry> _leaderboard = [];
    private MarathonProgress? _currentProgress;
    private bool _isLocked;
    private bool _isJoined;
    private bool _isLoading;
    private bool _hasSelection;

    public MarathonPageViewModel(
        IMarathonService marathonService,
        IMarathonRepository marathonRepository)
    {
        _marathonService = marathonService;
        _marathonRepository = marathonRepository;
    }

    // The display items for the top card row
    public ObservableCollection<MarathonDisplayItem> MarathonDisplayItems { get; } = new();

    public Marathon? SelectedMarathon
    {
        get => _selectedMarathon;
        private set => SetProperty(ref _selectedMarathon, value);
    }

    public IReadOnlyList<LeaderboardEntry> Leaderboard
    {
        get => _leaderboard;
        private set => SetProperty(ref _leaderboard, value);
    }

    public MarathonProgress? CurrentProgress
    {
        get => _currentProgress;
        private set
        {
            SetProperty(ref _currentProgress, value);
            OnPropertyChanged(nameof(ProgressText));
        }
    }

    public bool IsLocked
    {
        get => _isLocked;
        private set => SetProperty(ref _isLocked, value);
    }

    public bool IsJoined
    {
        get => _isJoined;
        private set
        {
            SetProperty(ref _isJoined, value);
            OnPropertyChanged(nameof(ShowJoinButton));
            OnPropertyChanged(nameof(ShowMovieList));
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set => SetProperty(ref _isLoading, value);
    }

    public bool HasSelection
    {
        get => _hasSelection;
        private set => SetProperty(ref _hasSelection, value);
    }

    public ObservableCollection<MarathonMovieItem> Movies { get; } = new();

    public bool ShowJoinButton => SelectedMarathon is not null && !IsJoined && !IsLocked;
    public bool ShowMovieList => SelectedMarathon is not null && IsJoined;

    public string ProgressText => CurrentProgress is null
        ? "Not joined yet"
        : CurrentProgress.IsCompleted
            ? $"✅ Completed — {CurrentProgress.CompletedMoviesCount} movies verified"
            : $"{CurrentProgress.CompletedMoviesCount} of {Movies.Count} movies verified";

    public async Task LoadAsync(int userId)
    {
        var marathons = await _marathonService.GetWeeklyMarathonsAsync(userId);

        MarathonDisplayItems.Clear();
        var weekEnd = GetSundayEnd();

        foreach (var marathon in marathons)
        {
            var participantCount = await _marathonRepository
                .GetParticipantCountAsync(marathon.Id);

            var progress = await _marathonRepository
                .GetUserProgressAsync(userId, marathon.Id);

            var totalMovies = await _marathonRepository
                .GetMarathonMovieCountAsync(marathon.Id);

            MarathonDisplayItems.Add(new MarathonDisplayItem
            {
                Marathon = marathon,
                ParticipantCount = participantCount,
                UserAccuracy = progress?.TriviaAccuracy ?? 0,
                IsJoinedByUser = progress is not null,
                UserMoviesVerified = progress?.CompletedMoviesCount ?? 0,
                TotalMovies = totalMovies,
                WeekEnd = weekEnd,
            });
        }
    }

    public async Task SelectMarathonAsync(Marathon marathon, int userId)
    {
        SelectedMarathon = marathon;
        HasSelection = true;
        IsLoading = true;

        try
        {
            CurrentProgress = await _marathonService
                .GetCurrentProgressAsync(marathon.Id);

            IsJoined = CurrentProgress is not null;

            IsLocked = false;
            if (marathon.PrerequisiteMarathonId is int prereqId)
            {
                var prereqDone = await _marathonRepository
                    .IsPrerequisiteCompletedAsync(userId, prereqId);
                IsLocked = !prereqDone;
            }

            var leaderboard = await _marathonRepository
                .GetLeaderboardWithUsernamesAsync(marathon.Id);
            Leaderboard = leaderboard.ToList();

            if (IsJoined)
                await LoadMoviesAsync(marathon.Id);
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task<bool> JoinMarathonAsync(int marathonId)
    {
        var success = await _marathonService.StartMarathonAsync(marathonId);
        if (!success) return false;

        CurrentProgress = await _marathonService.GetCurrentProgressAsync(marathonId);
        IsJoined = true;
        await LoadMoviesAsync(marathonId);
        return true;
    }

    public async Task RefreshAfterMovieLoggedAsync(int marathonId)
    {
        CurrentProgress = await _marathonService
            .GetCurrentProgressAsync(marathonId);

        var leaderboard = await _marathonRepository
            .GetLeaderboardWithUsernamesAsync(marathonId);
        Leaderboard = leaderboard.ToList();

        await LoadMoviesAsync(marathonId);
    }

    private async Task LoadMoviesAsync(int marathonId)
    {
        var movies = await _marathonRepository
            .GetMoviesForMarathonAsync(marathonId);

        var verifiedCount = CurrentProgress?.CompletedMoviesCount ?? 0;
        Movies.Clear();
        var movieList = movies.ToList();
        for (int i = 0; i < movieList.Count; i++)
        {
            Movies.Add(new MarathonMovieItem
            {
                MovieId = movieList[i].Id,
                Title = movieList[i].Title,
                IsVerified = i < verifiedCount,
            });
        }
        OnPropertyChanged(nameof(ProgressText));
    }

    private static DateTime GetSundayEnd()
    {
        var now = DateTime.UtcNow;
        var daysFromMonday = ((int)now.DayOfWeek + 6) % 7;
        var monday = now.Date.AddDays(-daysFromMonday);
        return monday.AddDays(6).AddHours(23).AddMinutes(59).AddSeconds(59);
    }
}