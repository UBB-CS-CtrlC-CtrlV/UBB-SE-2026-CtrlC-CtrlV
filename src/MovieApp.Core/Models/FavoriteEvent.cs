namespace MovieApp.Core.Models;

/// Link entity: a user can mark an event as favorite.
/// Mirrors the style used by Participation (UserId + EventId).
public sealed class FavoriteEvent
{
    public int Id { get; set; }

    public required int UserId { get; init; }

    public required int EventId { get; init; }

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public string FavoriteKey => $"U{UserId}:E{EventId}";
}
