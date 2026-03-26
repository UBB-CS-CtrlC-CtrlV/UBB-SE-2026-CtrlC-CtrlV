using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MovieApp.Core.Models;
using MovieApp.Core.Services;
using MovieApp.Infrastructure;
using MovieApp.Ui.ViewModels;

namespace MovieApp.Ui.Views;

public sealed partial class MarathonsPage : Page
{
    private MarathonTriviaViewModel? _triviaVm;
    private int _currentMovieId;

    public MarathonPageViewModel ViewModel { get; }

    public MarathonsPage()
    {
        // Build dependencies the same way HomePage and ReferralAreaPage do —
        // read the connection string from App.Configuration and create
        // the repo and service directly here.
        var connectionString = App.Configuration?["Database:ConnectionString"]
            ?? throw new InvalidOperationException("Missing connection string.");

        var db = new DatabaseOptions { ConnectionString = connectionString };
        var marathonRepo = new SqlMarathonRepository(db);
        var marathonService = new MarathonService(marathonRepo, App.CurrentUserService!);

        ViewModel = new MarathonPageViewModel(marathonService, marathonRepo);
        InitializeComponent();
        Loaded += async (_, _) => await ViewModel.LoadAsync();
    }

    // Called when the user selects a marathon from the left list
    private async void MarathonListView_SelectionChanged(
        object sender, SelectionChangedEventArgs e)
    {
        if (MarathonListView.SelectedItem is not Marathon marathon) return;

        await ViewModel.SelectMarathonAsync(marathon);

        // Show or hide the locked banner
        LockedBanner.Visibility = ViewModel.IsLocked
            ? Visibility.Visible
            : Visibility.Collapsed;

        // Reset the middle column back to idle when switching marathons
        ShowIdle();
    }

    // Call this from a "Log" button next to a movie in the marathon.
    // For now this is wired manually — pass the movieId of the movie to verify.
    public async Task StartQuizForMovieAsync(int movieId)
    {
        if (App.TriviaRepository is null) return;
        if (ViewModel.IsLocked) return;

        _currentMovieId = movieId;
        _triviaVm = new MarathonTriviaViewModel(App.TriviaRepository);

        await _triviaVm.StartAsync(movieId);

        ShowPlaying();
        RefreshQuizUi();
    }

    private void SubmitButton_Click(object sender, RoutedEventArgs e)
    {
        if (_triviaVm is null) return;

        // Find which radio button is checked and read its Tag (A/B/C/D)
        var selected = new[] { OptionA, OptionB, OptionC, OptionD }
            .FirstOrDefault(r => r.IsChecked == true);

        if (selected?.Tag is not char option) return;

        _triviaVm.SubmitAnswer(option);

        // Clear selection for next question
        foreach (var r in new[] { OptionA, OptionB, OptionC, OptionD })
            r.IsChecked = false;

        if (_triviaVm.IsComplete)
        {
            ShowResult();

            // If the user passed all 3, log the movie to the marathon
            if (_triviaVm.IsPassed && ViewModel.SelectedMarathon is not null)
            {
                _ = LogPassedMovieAsync(
                    ViewModel.SelectedMarathon.Id,
                    _currentMovieId,
                    _triviaVm.CorrectCount);
            }
        }
        else
        {
            RefreshQuizUi();
        }
    }

    private async Task LogPassedMovieAsync(int marathonId, int movieId, int correctCount)
    {
        var connectionString = App.Configuration?["Database:ConnectionString"]!;
        var db = new DatabaseOptions { ConnectionString = connectionString };
        var repo = new SqlMarathonRepository(db);
        var service = new MarathonService(repo, App.CurrentUserService!);

        await service.LogMovieAsync(marathonId, movieId, correctCount);

        // Refresh progress text and leaderboard in the ViewModel
        await ViewModel.RefreshAfterMovieLoggedAsync();
    }

    private async void TryAgainButton_Click(object sender, RoutedEventArgs e)
    {
        if (_triviaVm is null || App.TriviaRepository is null) return;
        _triviaVm.Reset();
        await _triviaVm.StartAsync(_currentMovieId);
        ShowPlaying();
        RefreshQuizUi();
    }

    // Updates the middle column quiz controls from the current ViewModel state
    private void RefreshQuizUi()
    {
        if (_triviaVm?.CurrentQuestion is not TriviaQuestion q) return;

        QuizProgress.Text = _triviaVm.ProgressText;
        QuizQuestion.Text = q.QuestionText;

        OptionA.Content = q.OptionA; OptionA.Tag = 'A';
        OptionB.Content = q.OptionB; OptionB.Tag = 'B';
        OptionC.Content = q.OptionC; OptionC.Tag = 'C';
        OptionD.Content = q.OptionD; OptionD.Tag = 'D';

        SubmitButton.IsEnabled = true;
    }

    // Panel state switching
    private void ShowIdle()
    {
        IdlePanel.Visibility = Visibility.Visible;
        PlayingPanel.Visibility = Visibility.Collapsed;
        ResultPanel.Visibility = Visibility.Collapsed;
    }

    private void ShowPlaying()
    {
        IdlePanel.Visibility = Visibility.Collapsed;
        PlayingPanel.Visibility = Visibility.Visible;
        ResultPanel.Visibility = Visibility.Collapsed;
    }

    private void ShowResult()
    {
        IdlePanel.Visibility = Visibility.Collapsed;
        PlayingPanel.Visibility = Visibility.Collapsed;
        ResultPanel.Visibility = Visibility.Visible;

        ResultText.Text = _triviaVm?.ResultText ?? string.Empty;

        // Only show Try Again if the user failed
        TryAgainButton.Visibility = (_triviaVm?.IsPassed == false)
            ? Visibility.Visible
            : Visibility.Collapsed;
    }
}