using Microsoft.UI.Xaml;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;
using MovieApp.Core.Services;

namespace MovieApp.Ui.ViewModels;

/// <summary>
/// Coordinates marathon selection, leaderboard loading, and current-user progress display.
/// </summary>
public sealed class MarathonPageViewModel : ViewModelBase
{
    private readonly IMarathonService? _marathonService;
    private readonly IMarathonRepository? _marathonRepository;
    private IReadOnlyList<Marathon> _marathons = [];
    private Marathon? _selectedMarathon;
    private IReadOnlyList<MarathonProgress> _leaderboard = [];
    private MarathonProgress? _currentProgress;
    private bool _isLocked;

    /// <summary>
    /// Creates the marathon page view model.
    /// </summary>
    public MarathonPageViewModel(
        IMarathonService marathonService,
        IMarathonRepository marathonRepository)
    {
        _marathonService = marathonService;
        _marathonRepository = marathonRepository;
    }

    /// <summary>
    /// Creates a marathon page view model whose data-backed features are unavailable.
    /// </summary>
    public MarathonPageViewModel()
    {
    }

    /// <summary>
    /// Gets the marathons currently loaded for the page.
    /// </summary>
    public IReadOnlyList<Marathon> Marathons
    {
        get => _marathons;
        private set => SetProperty(ref _marathons, value);
    }

    /// <summary>
    /// Gets the currently selected marathon.
    /// </summary>
    public Marathon? SelectedMarathon
    {
        get => _selectedMarathon;
        private set => SetProperty(ref _selectedMarathon, value);
    }

    /// <summary>
    /// Gets the leaderboard entries for the selected marathon.
    /// </summary>
    public IReadOnlyList<MarathonProgress> Leaderboard
    {
        get => _leaderboard;
        private set => SetProperty(ref _leaderboard, value);
    }

    /// <summary>
    /// Gets the current user's progress for the selected marathon.
    /// </summary>
    public MarathonProgress? CurrentProgress
    {
        get => _currentProgress;
        private set
        {
            SetProperty(ref _currentProgress, value);
            OnPropertyChanged(nameof(ProgressText));
        }
    }

    /// <summary>
    /// Gets a value indicating whether the selected marathon is locked.
    /// </summary>
    public bool IsLocked
    {
        get => _isLocked;
        private set => SetProperty(ref _isLocked, value);
    }

    /// <summary>
    /// Gets a value indicating whether marathon data is available from the configured services.
    /// </summary>
    public bool IsDataAvailable => _marathonService is not null && _marathonRepository is not null;

    /// <summary>
    /// Gets the visibility of the marathon availability message.
    /// </summary>
    public Visibility StatusVisibility => IsDataAvailable ? Visibility.Collapsed : Visibility.Visible;

    /// <summary>
    /// Gets the status message shown when marathon data cannot be loaded.
    /// </summary>
    public string StatusMessage => "Marathons are unavailable because the database connection is not ready.";

    /// <summary>
    /// Gets the text shown for the current marathon progress state.
    /// </summary>
    public string ProgressText => CurrentProgress is null
        ? "Not started"
        : CurrentProgress.IsCompleted
            ? $"Completed - {CurrentProgress.CompletedMoviesCount} movies verified"
            : $"{CurrentProgress.CompletedMoviesCount} movies verified so far";

    /// <summary>
    /// Loads the marathons available to the supplied user for the current week.
    /// </summary>
    public async Task LoadAsync(int userId)
    {
        if (!IsDataAvailable)
        {
            Marathons = [];
            return;
        }

        var list = await _marathonService!.GetWeeklyMarathonsAsync(userId);
        Marathons = list.ToList();
    }

    /// <summary>
    /// Selects a marathon and loads its progress and leaderboard state.
    /// </summary>
    public async Task SelectMarathonAsync(Marathon marathon)
    {
        if (!IsDataAvailable)
        {
            SelectedMarathon = null;
            CurrentProgress = null;
            Leaderboard = [];
            IsLocked = false;
            return;
        }

        SelectedMarathon = marathon;

        CurrentProgress = await _marathonService!
            .GetCurrentProgressAsync(marathon.Id);

        var leaderboard = await _marathonRepository!
            .GetLeaderboardAsync(marathon.Id);
        Leaderboard = leaderboard.ToList();

        IsLocked = false;
        if (marathon.PrerequisiteMarathonId is int prereqId
            && CurrentProgress is not null)
        {
            var prereqDone = await _marathonRepository
                .IsPrerequisiteCompletedAsync(CurrentProgress.UserId, prereqId);
            IsLocked = !prereqDone;
        }
    }

    /// <summary>
    /// Refreshes progress and leaderboard data after a movie is logged.
    /// </summary>
    public async Task RefreshAfterMovieLoggedAsync()
    {
        if (SelectedMarathon is null || !IsDataAvailable)
        {
            return;
        }

        CurrentProgress = await _marathonService!
            .GetCurrentProgressAsync(SelectedMarathon.Id);

        var leaderboard = await _marathonRepository!
            .GetLeaderboardAsync(SelectedMarathon.Id);
        Leaderboard = leaderboard.ToList();
    }
}
