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
    private DispatcherTimer? _countdownTimer;
    private DateTime _nextSpinTime;

    private void StartCountdown()
    {
        
        _nextSpinTime = DateTime.Today.AddDays(1);
        CountdownBanner.Visibility = Visibility.Visible;

        _countdownTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };

        _countdownTimer.Tick += (s, e) =>
        {
            var remaining = _nextSpinTime - DateTime.Now;
            if (remaining <= TimeSpan.Zero)
            {
                _countdownTimer.Stop();
                CountdownBanner.Visibility = Visibility.Collapsed;
                SpinButton.IsEnabled = true;
                RemainingSpinsText.Text = "1 spin available today";
            }
            else
            {
                CountdownText.Text = remaining.ToString(@"hh\:mm\:ss");
            }
        };

        _countdownTimer.Start();
    }

    private readonly string[] _categories = new[]
    {
        "Actors", "Directors", "Movie Quotes", "Oscars and Awards", "General Movie Trivia"
    };

    // Warm, vibrant, complementary palette
    private readonly Color[] _segmentColors = new[]
    {
        Color.FromArgb(255, 229,  57,  53),  // vivid red
        Color.FromArgb(255, 255, 160,   0),  // warm amber
        Color.FromArgb(255,  30, 136,  30),  // rich green
        Color.FromArgb(255,  21, 101, 192),  // deep blue
        Color.FromArgb(255, 142,  36, 170),  // bold purple
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

        if (_viewModel?.CanSpin == false)
        {
            RemainingSpinsText.Visibility = Visibility.Collapsed;
            StartCountdown();
        }
        else
        {
            RemainingSpinsText.Text = _viewModel?.RemainingSpinsText ?? "Loading...";
        }

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
                Stroke = new SolidColorBrush(Color.FromArgb(255, 20, 20, 20)),
                StrokeThickness = 2,
                Data = CreateSegmentGeometry(cx, cy, radius, startAngle, endAngle)
            };
            WheelCanvas.Children.Add(path);

            // Label centered on segment midpoint at 60% radius
            double midAngleRad = (startAngle + angleStep / 2.0) * Math.PI / 180.0;
            double labelRadius = radius * 0.60;
            double lx = cx + labelRadius * Math.Cos(midAngleRad) - 44;
            double ly = cy + labelRadius * Math.Sin(midAngleRad) - 14;

            // Shortened names so they fit inside the segment
            string shortName = _categories[i] switch
            {
                "Oscars and Awards" => "Oscars &\nAwards",
                "General Movie Trivia" => "General\nTrivia",
                "Movie Quotes" => "Movie\nQuotes",
                _ => _categories[i]
            };

            var label = new TextBlock
            {
                Text = shortName,
                FontSize = 11,
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                Width = 88,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
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

        var random = new Random();

        // Spin 3 full rotations plus a random extra angle
        double extraAngle = random.NextDouble() * 360.0;
        double totalRotation = 360.0 * 3 + extraAngle;

        // Figure out which segment the arrow (at top = 270°) points to after rotation
        // The wheel rotates clockwise, so we subtract from 270° to find where arrow lands
        double finalAngle = totalRotation % 360.0;
        double arrowPointsAt = (270.0 - finalAngle + 360.0) % 360.0;
        double segmentAngle = 360.0 / _categories.Length;
        int categoryIndex = (int)(arrowPointsAt / segmentAngle) % _categories.Length;

        var animation = new DoubleAnimation
        {
            From = 0,
            To = totalRotation,
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

        if (_viewModel!.HasEarnedReward)
        {
            ResultsTitleText.Text = "🎉 Perfect Score!";
            ResultsTitleText.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 200, 0)); // gold
            ResultsScoreText.Text = $"{_viewModel.Score}";
            ResultsScoreText.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 200, 0)); // gold
            ResultsRewardText.Text = "A free movie ticket reward has been added to your account!";
        }
        else
        {
            ResultsTitleText.Text = "Session Complete";
            ResultsTitleText.Foreground = new SolidColorBrush(Color.FromArgb(255, 200, 200, 200)); // neutral
            ResultsScoreText.Text = $"{_viewModel.Score}";
            ResultsScoreText.Foreground = new SolidColorBrush(Color.FromArgb(255, 200, 200, 200)); // neutral
            ResultsRewardText.Text = "Answer all 20 correctly next time to earn a reward.";
        }
    }
}