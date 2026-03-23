using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Shapes;
using MovieApp.Ui.ViewModels;
using Windows.UI;

namespace MovieApp.Ui.Views;

public sealed partial class TriviaWheelPage : Page
{
    private TriviaWheelViewModel? _viewModel;

    // 5 segments, each 72 degrees
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

            // Draw segment as a Path
            var path = new Path
            {
                Fill = new SolidColorBrush(_segmentColors[i]),
                Stroke = new SolidColorBrush(Color.FromArgb(255, 30, 30, 30)),
                StrokeThickness = 1,
                Data = CreateSegmentGeometry(cx, cy, radius, startAngle, endAngle)
            };
            WheelCanvas.Children.Add(path);

            // Add category label
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

        // Pick a random category
        var random = new Random();
        int categoryIndex = random.Next(_categories.Length);

        // Spin animation — 3 full rotations + land on segment
        double segmentAngle = 360.0 / _categories.Length;
        double targetAngle = 360.