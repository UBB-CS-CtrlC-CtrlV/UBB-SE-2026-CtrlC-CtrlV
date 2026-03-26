using MovieApp.Core.Models;
using MovieApp.Core.Repositories;
using MovieApp.Core.Services;

namespace MovieApp.Ui.ViewModels;

public sealed class MarathonPageViewModel : ViewModelBase
{
    private readonly IMarathonService _marathonService;
    private readonly IMarathonRepository _marathonRepository;
    private IReadOnlyList<Marathon> _marathons = [];
    private Marathon? _selectedMarathon;
    private IReadOnlyList<MarathonProgress> _leaderboard = [];
    private MarathonProgress? _currentProgress;
    private bool _isLocked;

    public MarathonPageViewModel(
        IMarathonService marathonService,
        IMarathonRepository marathonRepository)
    {
        _marathonService = marathonService;
        _marathonRepository = marathonRepository;
    }

    public IReadOnlyList<Marathon> Marathons
    {
        get => _marathons;
        private set => SetProperty(ref _marathons, value);
    }

    public Marathon? SelectedMarathon
    {
        get => _selectedMarathon;
        private set => SetProperty(ref _selectedMarathon, value);
    }

    public IReadOnlyList<MarathonProgress> Leaderboard
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

    public string ProgressText => CurrentProgress is null
        ? "Not started"
        : CurrentProgress.IsCompleted
            ? $"Completed — {CurrentProgress.CompletedMoviesCount} movies verified"
            : $"{CurrentProgress.CompletedMoviesCount} movies verified so far";

    public async Task LoadAsync()
    {
        var list = await _marathonService.GetWeeklyMarathonsAsync();
        Marathons = list.ToList();
    }

    public async Task SelectMarathonAsync(Marathon marathon)
    {
        SelectedMarathon = marathon;

        CurrentProgress = await _marathonService
            .GetCurrentProgressAsync(marathon.Id);

        var leaderboard = await _marathonRepository
            .GetLeaderboardAsync(marathon.Id);
        Leaderboard = leaderboard.ToList();

        // Check if this marathon is locked behind a prerequisite
        IsLocked = false;
        if (marathon.PrerequisiteMarathonId is int prereqId
            && CurrentProgress is not null)
        {
            var prereqDone = await _marathonRepository
                .IsPrerequisiteCompletedAsync(CurrentProgress.UserId, prereqId);
            IsLocked = !prereqDone;
        }
    }

    public async Task RefreshAfterMovieLoggedAsync()
    {
        if (SelectedMarathon is null) return;

        CurrentProgress = await _marathonService
            .GetCurrentProgressAsync(SelectedMarathon.Id);

        var leaderboard = await _marathonRepository
            .GetLeaderboardAsync(SelectedMarathon.Id);
        Leaderboard = leaderboard.ToList();
    }
}