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
        // Temporary local composition for the marathon feature.
        var connectionString = App.Configuration?["Database:ConnectionString"]
            ?? throw new InvalidOperationException("Missing connection string.");

        var db = new DatabaseOptions { ConnectionString = connectionString };
        var marathonRepo = new SqlMarathonRepository(db);
        var marathonService = new MarathonService(marathonRepo, App.CurrentUserService!);

        ViewModel = new MarathonPageViewModel(marathonService, marathonRepo);
        InitializeComponent();
        Loaded += async (_, _) =>
        {
            var userId = App.CurrentUserService?.CurrentUser.Id ?? 0;
            await ViewModel.LoadAsync(userId);
        };
    }

    private async void MarathonListView_SelectionChanged(
        object sender, SelectionChangedEventArgs e)
    {
        if (MarathonListView.SelectedItem is not Marathon marathon) return;

        await ViewModel.SelectMarathonAsync(marathon);

        LockedBanner.Visibility = ViewModel.IsLocked
            ? Visibility.Visible
            : Visibility.Collapsed;

        ShowIdle();
    }

    /// <summary>
    /// Starts the quiz flow for a specific marathon movie.
    /// </summary>
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

        var selected = new[] { OptionA, OptionB, OptionC, OptionD }
            .FirstOrDefault(r => r.IsChecked == true);

        if (selected?.Tag is not char option) return;

        _triviaVm.SubmitAnswer(option);

        foreach (var r in new[] { OptionA, OptionB, OptionC, OptionD })
            r.IsChecked = false;

        if (_triviaVm.IsComplete)
        {
            ShowResult();

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
