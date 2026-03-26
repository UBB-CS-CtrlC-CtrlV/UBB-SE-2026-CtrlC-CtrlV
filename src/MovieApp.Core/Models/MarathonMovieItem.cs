using Microsoft.UI.Xaml;

namespace MovieApp.Core.Models;

public sealed class MarathonMovieItem
{
    public required int MovieId { get; init; }
    public required string Title { get; init; }
    public bool IsVerified { get; set; }

    public string StatusText => IsVerified ? "Verified ✓" : "Not verified";
    public string LogButtonText => IsVerified ? "Done" : "Log";
    public bool CanLog => !IsVerified;
    public Visibility VerifiedVisibility => IsVerified ? Visibility.Visible : Visibility.Collapsed;
    public Visibility LogVisibility => IsVerified ? Visibility.Collapsed : Visibility.Visible;
}