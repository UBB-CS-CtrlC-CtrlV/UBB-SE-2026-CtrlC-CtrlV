namespace MovieApp.Core.Models;

public sealed class MarathonMovieItem
{
    public required int MovieId { get; init; }
    public required string Title { get; init; }
    public bool IsVerified { get; set; }

    public string StatusText => IsVerified ? "Verified ✓" : "Not verified";
    public bool CanLog => !IsVerified;
    public double IsVerifiedOpacity => IsVerified ? 1.0 : 0.0;
    public double CanLogOpacity => IsVerified ? 0.0 : 1.0;
}