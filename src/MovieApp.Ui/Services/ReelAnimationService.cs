using MovieApp.Core.Models.Movie;

namespace MovieApp.Ui.Services;

/// <summary>
/// Service responsible for managing reel animation timing and sequencing in WinUI.
/// Coordinates the timing of reel animations and provides animation state to the ViewModel.
/// </summary>
public sealed class ReelAnimationService
{
    private const int AnimationDurationMs = 2000;
    private const int ReelStopDelayMs = 200;

    public event EventHandler<ReelAnimationCompletedEventArgs>? AnimationCompleted;

    /// <summary>
    /// Animates all 3 reels with sequential stops.
    /// Reels stop in order: Genre, Actor, Director.
    /// During animation, each reel displays multiple sequential values.
    /// Final values are set to the generated values from the service.
    /// </summary>
    public async Task AnimateReelsAsync(
        Genre finalGenre,
        Actor finalActor,
        Director finalDirector,
        List<Genre> genreSequence,
        List<Actor> actorSequence,
        List<Director> directorSequence,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Start animations for all reels simultaneously
            var animationGenreTask = AnimateReelAsync(genreSequence.Cast<object>().ToList(), 0, cancellationToken);
            var animationActorTask = AnimateReelAsync(actorSequence.Cast<object>().ToList(), ReelStopDelayMs, cancellationToken);
            var animationDirectorTask = AnimateReelAsync(directorSequence.Cast<object>().ToList(), ReelStopDelayMs * 2, cancellationToken);

            await Task.WhenAll(animationGenreTask, animationActorTask, animationDirectorTask);

            // Notify completion with final values
            OnAnimationCompleted(new ReelAnimationCompletedEventArgs
            {
                FinalGenre = finalGenre,
                FinalActor = finalActor,
                FinalDirector = finalDirector,
                CompletedAt = DateTime.UtcNow
            });
        }
        catch (OperationCanceledException)
        {
            // Handle cancellation gracefully
        }
    }

    /// <summary>
    /// Animates a single reel by cycling through values for the animation duration.
    /// </summary>
    private async Task AnimateReelAsync(List<object> values, int delayMs, CancellationToken cancellationToken)
    {
        if (values.Count == 0)
            return;

        // Initial delay before this reel starts
        await Task.Delay(delayMs, cancellationToken);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        while (stopwatch.ElapsedMilliseconds < AnimationDurationMs && !cancellationToken.IsCancellationRequested)
        {
            // Cycle through reel values
            var index = (int)((stopwatch.ElapsedMilliseconds / 50) % values.Count);
            // Here you would update the UI binding with values[index]
            // This would be connected to the ViewModel that updates display

            await Task.Delay(50, cancellationToken);
        }

        stopwatch.Stop();
    }

    private void OnAnimationCompleted(ReelAnimationCompletedEventArgs e)
    {
        AnimationCompleted?.Invoke(this, e);
    }
}

/// <summary>
/// Event args for when reel animation completes.
/// </summary>
public sealed class ReelAnimationCompletedEventArgs : EventArgs
{
    public required Genre FinalGenre { get; init; }
    public required Actor FinalActor { get; init; }
    public required Director FinalDirector { get; init; }
    public required DateTime CompletedAt { get; init; }
}
