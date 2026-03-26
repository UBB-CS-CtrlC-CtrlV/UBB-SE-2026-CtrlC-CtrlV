using MovieApp.Core.Models;
using MovieApp.Core.Models.Movie;
using MovieApp.Core.Services;
using MovieApp.Ui.Controls;
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
    private readonly SlotMachineService _slotMachineService;
    private readonly SlotMachineAnimationService _animationService;
    private readonly int _userId;

    private Genre _selectedGenre = new();
    private Actor _selectedActor = new();
    private Director _selectedDirector = new();
    private int _availableSpins;
    private int _bonusSpins;
    private int _loginStreak;
    private bool _isSpinning;
    private bool _isSpinButtonEnabled;
    private bool _isGenreSpinning;
    private bool _isActorSpinning;
    private bool _isDirectorSpinning;
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

    public bool IsGenreSpinning
    {
        get => _isGenreSpinning;
        private set => SetProperty(ref _isGenreSpinning, value);
    }

    public bool IsActorSpinning
    {
        get => _isActorSpinning;
        private set => SetProperty(ref _isActorSpinning, value);
    }

    public bool IsDirectorSpinning
    {
        get => _isDirectorSpinning;
        private set => SetProperty(ref _isDirectorSpinning, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public ObservableCollection<MatchingEventItem> MatchingEvents { get; } = new();
    public Movie? JackpotMovie { get; private set; }
    public bool JackpotAchieved { get; private set; }

    private bool _hasMatchingEvents;
    public bool HasMatchingEvents
    {
        get => _hasMatchingEvents;
        private set => SetProperty(ref _hasMatchingEvents, value);
    }

    private bool _hasNoMatchingEvents = true;
    public bool HasNoMatchingEvents
    {
        get => _hasNoMatchingEvents;
        private set => SetProperty(ref _hasNoMatchingEvents, value);
    }

    public event Action<Movie, int>? JackpotHit;

    private AsyncRelayCommand? _spinCommand;
    public ICommand SpinCommand => _spinCommand ??= new AsyncRelayCommand(SpinAsync, CanSpin);

    /// <summary>
    /// Creates a database-backed slot-machine view model for the current user.
    /// </summary>
    public SlotMachineViewModel(
        int userId,
        SlotMachineService slotMachineService,
        SlotMachineAnimationService animationService)
    {
        _userId = userId;
        _slotMachineService = slotMachineService;
        _animationService = animationService;
        MatchingEvents.CollectionChanged += (_, _) =>
        {
            HasMatchingEvents = MatchingEvents.Count > 0;
            HasNoMatchingEvents = !HasMatchingEvents;
        };
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await LoadUserStateAsync(cancellationToken);
            UpdateIsSpinButtonEnabled();
            StatusMessage = App.StreakSpinGrantedOnLogin
                ? "🔥 3-day streak! Bonus spin awarded. Ready to spin!"
                : "Ready to spin!";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error initializing: {ex.Message}";
        }
    }

    /// <summary>
    /// Lightweight refresh of the spin counter only (SM.30).
    /// Call after an external action may have changed the user's available spins.
    /// </summary>
    public async Task RefreshSpinCountAsync()
    {
        AvailableSpins = await _slotMachineService.GetAvailableSpinsAsync(_userId);
    }

    private async Task LoadUserStateAsync(CancellationToken cancellationToken = default)
    {
        var availableSpins = await _slotMachineService.GetAvailableSpinsAsync(_userId);
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
        IsGenreSpinning = true;
        IsActorSpinning = true;
        IsDirectorSpinning = true;
        MatchingEvents.Clear();
        JackpotAchieved = false;
        StatusMessage = "Spinning...";

        try
        {
            // Perform the spin
            var result = await _slotMachineService.SpinAsync(_userId);

            // Get reel sequences for animation
            var genres = await _slotMachineService.GetGenresAsync();
            var actors = await _slotMachineService.GetActorsAsync();
            var directors = await _slotMachineService.GetDirectorsAsync();

            // Animate the reels with sequential stops: Genre -> Actor -> Director
            await _animationService.AnimateSpinAsync(
                result.Genre,
                result.Actor,
                result.Director,
                genres,
                actors,
                directors,
                genre => SelectedGenre = genre,
                actor => SelectedActor = actor,
                director => SelectedDirector = director,
                reelIndex =>
                {
                    switch (reelIndex)
                    {
                        case 0: IsGenreSpinning = false; break;
                        case 1: IsActorSpinning = false; break;
                        case 2: IsDirectorSpinning = false; break;
                    }
                });

            // Update UI with results (reel values already set by animation callbacks)
            JackpotMovie = result.JackpotMovie;
            JackpotAchieved = result.JackpotDiscountApplied;

            // Populate matching events with jackpot highlighting
            MatchingEvents.Clear();
            foreach (var evt in result.MatchingEvents)
            {
                var isJackpot = result.JackpotEventIds.Contains(evt.Id);
                MatchingEvents.Add(new MatchingEventItem(evt, isJackpot));
            }

            // Update status
            if (JackpotAchieved)
            {
                StatusMessage = $"🎉 JACKPOT! {result.DiscountPercentage}% discount earned on {result.JackpotMovie?.Title}!";
                JackpotHit?.Invoke(result.JackpotMovie!, result.DiscountPercentage);
            }
            else if (MatchingEvents.Count > 0)
            {
                StatusMessage = $"Found {MatchingEvents.Count} matching events!";
            }
            else
            {
                StatusMessage = "No matching events this time. Try again!";
            }

            // Refresh available spins (without overwriting reel values)
            AvailableSpins = await _slotMachineService.GetAvailableSpinsAsync(_userId);
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
/// Wraps an Event with a jackpot indicator for display in the matching events list.
/// </summary>
public sealed class MatchingEventItem
{
    public Event Event { get; }
    public bool IsJackpotEvent { get; }
    public string PriceText => EventCard.GetPriceText(Event, System.Globalization.CultureInfo.CurrentCulture);

    public MatchingEventItem(Event evt, bool isJackpotEvent)
    {
        Event = evt;
        IsJackpotEvent = isJackpotEvent;
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
