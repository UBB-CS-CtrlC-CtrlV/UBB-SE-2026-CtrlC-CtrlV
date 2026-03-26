using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MovieApp.Core.Models;
using MovieApp.Core.Services;
using MovieApp.Ui.ViewModels;

namespace MovieApp.Ui.Views;

/// <summary>
/// Hosts the marathon browsing, quiz, and leaderboard surface while deferring
/// data-backed interactions when the shared database services are unavailable.
/// </summary>
public sealed partial class MarathonsPage : Page
{
    private readonly IMarathonService? _marathonService;
    private MarathonTriviaViewModel? _triviaVm;
    private int _currentMovieId;

    /// <summary>
    /// Gets the page-level marathon view model.
    /// </summary>
    public MarathonPageViewModel ViewModel { get; }

    public MarathonsPage()
    {
        if (App.MarathonRepository is not null && App.CurrentUserService is not null)
        {
            _marathonService = new MarathonService(App.MarathonRepository, App.CurrentUserService);
            ViewModel = new MarathonPageViewModel(_marathonService, App.MarathonRepository);
        }
        else
        {
            ViewModel = new MarathonPageViewModel();
        }

        InitializeComponent();
        Loaded += OnPageLoaded;
    }

    private async void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnPageLoaded;

        var userId = App.CurrentUserService?.CurrentUser.Id ?? 0;
        await ViewModel.LoadAsync(userId);
    }

    private async void MarathonListView_SelectionChanged(
        object sender, SelectionChangedEventArgs e)
    {
        if (MarathonListView.SelectedItem is not Marathon marathon)
        {
            return;
        }

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
        if (App.TriviaRepository is null || ViewModel.IsLocked || !ViewModel.IsDataAvailable)
        {
            return;
        }

        _currentMovieId = movieId;
        _triviaVm = new MarathonTriviaViewModel(App.TriviaRepository);

        await _triviaVm.StartAsync(movieId);

        ShowPlaying();
        RefreshQuizUi();
    }

    private void SubmitButton_Click(object sender, RoutedEventArgs e)
    {
        if (_triviaVm is null)
        {
            return;
        }

        var selected = new[] { OptionA, OptionB, OptionC, OptionD }
            .FirstOrDefault(radioButton => radioButton.IsChecked == true);

        if (selected?.Tag is not char option)
        {
            return;
        }

        _triviaVm.SubmitAnswer(option);

        foreach (var radioButton in new[] { OptionA, OptionB, OptionC, OptionD })
        {
            radioButton.IsChecked = false;
        }

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
        if (_marathonService is null)
        {
            return;
        }

        await _marathonService.LogMovieAsync(marathonId, movieId, correctCount);
        await ViewModel.RefreshAfterMovieLoggedAsync();
    }

    private async void TryAgainButton_Click(object sender, RoutedEventArgs e)
    {
        if (_triviaVm is null || App.TriviaRepository is null)
        {
            return;
        }

        _triviaVm.Reset();
        await _triviaVm.StartAsync(_currentMovieId);
        ShowPlaying();
        RefreshQuizUi();
    }

    private void RefreshQuizUi()
    {
        if (_triviaVm?.CurrentQuestion is not TriviaQuestion question)
        {
            return;
        }

        QuizProgress.Text = _triviaVm.ProgressText;
        QuizQuestion.Text = question.QuestionText;

        OptionA.Content = question.OptionA;
        OptionA.Tag = 'A';
        OptionB.Content = question.OptionB;
        OptionB.Tag = 'B';
        OptionC.Content = question.OptionC;
        OptionC.Tag = 'C';
        OptionD.Content = question.OptionD;
        OptionD.Tag = 'D';

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
        TryAgainButton.Visibility = (_triviaVm?.IsPassed == false)
            ? Visibility.Visible
            : Visibility.Collapsed;
    }
}
