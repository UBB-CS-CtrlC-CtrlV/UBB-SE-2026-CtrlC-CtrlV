using MovieApp.Core.Models;
using MovieApp.Core.Models.Movie;
using MovieApp.Core.Services;
using MovieApp.Ui.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.UI.Xaml.Controls;

namespace MovieApp.Ui.ViewModels;

/// <summary>
/// ViewModel for the Slot Machine page.
/// Manages spin logic, reel display, and event results.
/// </summary>
public sealed class SlotMachineViewModel : ViewModelBase
{
    private readonly SlotMachineService? _slotMachineService;
    private readonly SlotMachineResultService? _resultService;
    private readonly ReelAnimationService? _animationService;
    private readonly int _userId;

    private Genre _selectedGenre = new();
    private Actor _selectedActor = new();
    private Director _selectedDirector = new();
    private int _availableSpins;
    private int _bonusSpins;
    private int _loginStreak;
    private bool _isSpinning;
    private bool _isSpinButtonEnabled;
    private string _statusMessage = "Ready to spin!";

    public Genre SelectedGenre
    {
        get => _selectedGenre;
        private set => SetProperty(ref _selectedGenre, value);
    }

    public Actor SelectedActor
    {
        get => _selectedActor;
        private set => SetProperty(ref _selectedActor, value);
    }

    public Director SelectedDirector
    {
        get => _selectedDirector;
        private set => SetProperty(ref _selectedDirector, value);
    }

    public int AvailableSpins
    {
        get => _availableSpins;
        private set
        {
            if (SetProperty(ref _availableSpins, value))
                UpdateIsSpinButtonEnabled();
        }
    }

    public int BonusSpins
    {
        get => _bonusSpins;
        private set => SetProperty(ref _bonusSpins, value);
    }

    public int LoginStreak
    {
        get => _loginStreak;
        private set => SetProperty(ref _loginStreak, value);
    }

    public bool IsSpinning
    {
        get => _isSpinning;
        private set
        {
            if (SetProperty(ref _isSpinning, value))
                UpdateIsSpinButtonEnabled();
        }
    }

    public bool IsSpinButtonEnabled
    {
        get => _isSpinButtonEnabled;
        private set => SetProperty(ref _isSpinButtonEnabled, value);
    }

    private void UpdateIsSpinButtonEnabled()
    {
        IsSpinButtonEnabled = !IsSpinning && AvailableSpins > 0;
        _spinCommand?.NotifyCanExecuteChanged();
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public ObservableCollection<Event> MatchingEvents { get; } = new();
    public Movie? JackpotMovie { get; private set; }
    public bool JackpotAchieved { get; private set; }

    private AsyncRelayCommand? _spinCommand;
    public ICommand SpinCommand => _spinCommand ??= new AsyncRelayCommand(SpinAsync, CanSpin);

    /// <summary>
    /// Creates a database-backed slot-machine view model for the current user.
    /// </summary>
    public SlotMachineViewModel(
        int userId,
        SlotMachineService slotMachineService,
        SlotMachineResultService resultService,
        ReelAnimationService animationService)
    {
        _userId = userId;
        _slotMachineService = slotMachineService;
        _resultService = resultService;
        _animationService = animationService;
    }

    private SlotMachineViewModel(string unavailableMessage)
    {
        _statusMessage = unavailableMessage;
        _isSpinButtonEnabled = false;
    }

    /// <summary>
    /// Creates a non-interactive view model used when the database-backed slot-machine
    /// services are unavailable but the shell should still remain navigable.
    /// </summary>
    /// <param name="unavailableMessage">Short message explaining why the feature is offline.</param>
    /// <returns>A disabled slot-machine state for the page.</returns>
    public static SlotMachineViewModel CreateUnavailable(string unavailableMessage)
    {
        return new SlotMachineViewModel(unavailableMessage);
    }

    /// <summary>
    /// Loads the current user's slot-machine state and the initial reel values.
    /// </summary>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await LoadUserStateAsync(cancellationToken);
            UpdateIsSpinButtonEnabled();
            StatusMessage = "Ready to spin!";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error initializing: {ex.Message}";
        }
    }

    private async Task LoadUserStateAsync(CancellationToken cancellationToken = default)
    {
        var availableSpins = await _slotMachineService!.GetAvailableSpinsAsync(_userId);
        AvailableSpins = availableSpins;

        // Load initial random values for display
        SelectedGenre = await _slotMachineService.GetRandomGenreAsync(cancellationToken);
        SelectedActor = await _slotMachineService.GetRandomActorAsync(cancellationToken);
        SelectedDirector = await _slotMachineService.GetRandomDirectorAsync(cancellationToken);
    }

    private bool CanSpin() => !IsSpinning && AvailableSpins > 0;

    private async Task SpinAsync()
    {
        if (IsSpinning || AvailableSpins <= 0)
            return;

        IsSpinning = true;
        MatchingEvents.Clear();
        JackpotAchieved = false;
        StatusMessage = "Spinning...";

        try
        {
            // Perform the spin
            var result = await _slotMachineService!.SpinAsync(_userId);

            // Get reel sequences for animation
            var genres = await _slotMachineService.GetGenresAsync();
            var actors = await _slotMachineService.GetActorsAsync();
            var directors = await _slotMachineService.GetDirectorsAsync();

            // Animate the reels
            await _animationService!.AnimateReelsAsync(
                result.Genre,
                result.Actor,
                result.Director,
                genres.ToList(),
                actors.ToList(),
                directors.ToList());

            // Prepare final result
            var finalResult = await _resultService!.PrepareSpinResultAsync(
                _userId,
                result.Genre,
                result.Actor,
                result.Director,
                result.MatchingEvents,
                result.JackpotMovie);

            // Update UI with results
            SelectedGenre = finalResult.Genre;
            SelectedActor = finalResult.Actor;
            SelectedDirector = finalResult.Director;
            JackpotMovie = finalResult.JackpotMovie;
            JackpotAchieved = finalResult.JackpotDiscountApplied;

            // Populate matching events
            MatchingEvents.Clear();
            foreach (var evt in finalResult.MatchingEvents)
            {
                MatchingEvents.Add(evt);
            }

            // Update status
            if (JackpotAchieved)
            {
                StatusMessage = $"🎉 JACKPOT! {finalResult.DiscountPercentage}% discount earned on {finalResult.JackpotMovie?.Title}!";
            }
            else if (MatchingEvents.Count > 0)
            {
                StatusMessage = $"Found {MatchingEvents.Count} matching events!";
            }
            else
            {
                StatusMessage = "No matching events this time. Try again!";
            }

            // Refresh available spins
            await LoadUserStateAsync();
        }
        catch (InvalidOperationException ex)
        {
            StatusMessage = ex.Message;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error during spin: {ex.Message}";
        }
        finally
        {
            IsSpinning = false;
            ((AsyncRelayCommand)SpinCommand).NotifyCanExecuteChanged();
        }
    }
}

/// <summary>
/// Simple async relay command implementation for MVVM.
/// </summary>
public sealed class AsyncRelayCommand : ICommand
{
    private readonly Func<Task> _executeAsync;
    private readonly Func<bool> _canExecute;

    public event EventHandler? CanExecuteChanged;

    public AsyncRelayCommand(Func<Task> executeAsync, Func<bool>? canExecute = null)
    {
        _executeAsync = executeAsync;
        _canExecute = canExecute ?? (() => true);
    }

    public bool CanExecute(object? parameter) => _canExecute();

    public async void Execute(object? parameter)
    {
        if (CanExecute(parameter))
        {
            await _executeAsync();
        }
    }

    public void NotifyCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
