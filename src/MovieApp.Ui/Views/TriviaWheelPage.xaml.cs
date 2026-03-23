using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Shapes;
using MovieApp.Ui.ViewModels;
using Windows.UI;
using Path = Microsoft.UI.Xaml.Shapes.Path;

namespace MovieApp.Ui.Views;

public sealed partial class TriviaWheelPage : Page
{
    private TriviaWheelViewModel? _viewModel;

    private readonly string[] _categories = new[]
    {
        "Actors", "Directors", "Movie Quotes", "Oscars and Awards", "General Movie Trivia"
    };

    private readonly Color[] _segmentColors = new[]
    {
        Color.FromArgb(255, 99,  179, 237),
        Color.FromArgb(255, 154, 117, 234),
        Color.FromArgb(255, 72,  187, 120),
        Color.FromArgb(255, 246, 173, 85),
        Color.FromArgb(255, 237, 100, 166),
    };

    public TriviaWheelPage()
    {
        InitializeComponent();
        Loaded += OnPageLoaded;
    }

    private void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        if (App.TriviaRepository is not null)
        {
            _viewModel = new TriviaWheelViewModel(App.TriviaRepository);
        }

        RemainingSpinsText.Text = _viewModel?.RemainingSpinsText ?? "Loading...";
        SpinButton.IsEnabled = _viewModel?.CanSpin ?? false;

        DrawWheel();
    }

    private void DrawWheel()
    {
        WheelCanvas.Children.Clear();
        double cx = 140, cy = 140, radius = 130;
        double angleStep = 360.0 / _categories.Length;

        for (int i = 0; i < _categories.Length; i++)
        {
            double startAngle = i * angleStep;
            double endAngle = startAngle + angleStep;

            var path = new Path
            {
                Fill = new SolidColorBrush(_segmentColors[i]),
                Stroke = new SolidColorBrush(Color.FromArgb(255, 30, 30, 30)),
                StrokeThickness = 1,
                Data = CreateSegmentGeometry(cx, cy, radius, startAngle, endAngle)
            };
            WheelCanvas.Children.Add(path);

            double midAngle = (startAngle + endAngle) / 2.0 * Math.PI / 180.0;
            double labelRadius = radius * 0.65;
            double lx = cx + labelRadius * Math.Cos(midAngle) - 40;
            double ly = cy + labelRadius * Math.Sin(midAngle) - 10;

            var label = new TextBlock
            {
                Text = _categories[i],
                FontSize = 9,
                Width = 80,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255))
            };

            Canvas.SetLeft(label, lx);
            Canvas.SetTop(label, ly);
            WheelCanvas.Children.Add(label);
        }
    }

    private static PathGeometry CreateSegmentGeometry(
        double cx, double cy, double radius,
        double startDeg, double endDeg)
    {
        double startRad = startDeg * Math.PI / 180.0;
        double endRad = endDeg * Math.PI / 180.0;

        var startPoint = new Windows.Foundation.Point(
            cx + radius * Math.Cos(startRad),
            cy + radius * Math.Sin(startRad));

        var endPoint = new Windows.Foundation.Point(
            cx + radius * Math.Cos(endRad),
            cy + radius * Math.Sin(endRad));

        var figure = new PathFigure
        {
            StartPoint = new Windows.Foundation.Point(cx, cy),
            IsClosed = true
        };

        figure.Segments.Add(new LineSegment { Point = startPoint });
        figure.Segments.Add(new ArcSegment
        {
            Point = endPoint,
            Size = new Windows.Foundation.Size(radius, radius),
            IsLargeArc = (endDeg - startDeg) > 180,
            SweepDirection = SweepDirection.Clockwise
        });

        var geometry = new PathGeometry();
        geometry.Figures.Add(figure);
        return geometry;
    }

    private void SpinButton_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel is null || !_viewModel.CanSpin) return;

        SpinButton.IsEnabled = false;
        SelectedCategoryText.Text = "Spinning...";

        var random = new Random();
        int categoryIndex = random.Next(_categories.Length);

        double segmentAngle = 360.0 / _categories.Length;
        double targetAngle = 360.0 * 3 + (categoryIndex * segmentAngle);

        var animation = new DoubleAnimation
        {
            From = 0,
            To = targetAngle,
            Duration = new Duration(TimeSpan.FromSeconds(3)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        var storyboard = new Storyboard();
        storyboard.Children.Add(animation);
        Storyboard.SetTarget(animation, WheelRotation);
        Storyboard.SetTargetProperty(animation, "Angle");

        storyboard.Completed += (s, ev) =>
        {
            SelectedCategoryText.Text = _categories[categoryIndex];
            ShowPlayingPanel();
            _ = LoadQuestionsAsync(_categories[categoryIndex]);
        };

        storyboard.Begin();
    }

    private async Task LoadQuestionsAsync(string category)
    {
        if (_viewModel is null) return;
        await _viewModel.LoadQuestionsAsync(category);
        ShowCurrentQuestion();
    }

    private void ShowCurrentQuestion()
    {
        if (_viewModel?.CurrentQuestion is null) return;

        var q = _viewModel.CurrentQuestion;
        QuestionText.Text = q.QuestionText;
        OptionA.Content = q.OptionA;
        OptionB.Content = q.OptionB;
        OptionC.Content = q.OptionC;
        OptionD.Content = q.OptionD;

        OptionA.IsChecked = false;
        OptionB.IsChecked = false;
        OptionC.IsChecked = false;
        OptionD.IsChecked = false;

        OptionA.Visibility = Visibility.Visible;
        OptionB.Visibility = Visibility.Visible;
        OptionC.Visibility = Visibility.Visible;
        OptionD.Visibility = Visibility.Visible;

        ProgressText.Text = _viewModel.ProgressText;
        ProgressBar.Value = _viewModel.ProgressValue;
        HintButton.IsEnabled = _viewModel.IsHintAvailable;
    }

    private void HintButton_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel is null) return;

        _viewModel.UseHint();
        HintButton.IsEnabled = false;

        var toHide = _viewModel.GetHintOptionsToHide();
        foreach (var option in toHide)
        {
            switch (option)
            {
                case 'A': OptionA.Visibility = Visibility.Collapsed; break;
                case 'B': OptionB.Visibility = Visibility.Collapsed; break;
                case 'C': OptionC.Visibility = Visibility.Collapsed; break;
                case 'D': OptionD.Visibility = Visibility.Collapsed; break;
            }
        }
    }

    private void SubmitAnswer_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel is null) return;

        char? selected = null;
        if (OptionA.IsChecked == true) selected = 'A';
        else if (OptionB.IsChecked == true) selected = 'B';
        else if (OptionC.IsChecked == true) selected = 'C';
        else if (OptionD.IsChecked == true) selected = 'D';

        if (selected is null) return;

        _viewModel.SubmitAnswer(selected.Value);

        if (_viewModel.IsSessionComplete)
            ShowResults();
        else
            ShowCurrentQuestion();
    }

    private void ShowPlayingPanel()
    {
        IdlePanel.Visibility = Visibility.Collapsed;
        ResultsPanel.Visibility = Visibility.Collapsed;
        PlayingPanel.Visibility = Visibility.Visible;
    }

    private void ShowResults()
    {
        PlayingPanel.Visibility = Visibility.Collapsed;
        ResultsPanel.Visibility = Visibility.Visible;

        ResultsTitleText.Text = _viewModel!.HasEarnedReward ? "🎉 You won!" : "Session Complete";
        ResultsScoreText.Text = $"You answered {_viewModel.Score}/20 correctly.";
        ResultsRewardText.Text = _viewModel.HasEarnedReward
            ? "A free movie ticket reward has been added to your account!"
            : "Answer all 20 correctly next time to earn a reward.";
    }
}