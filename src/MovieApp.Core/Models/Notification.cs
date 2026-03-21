namespace MovieApp.Core.Models;


/// Simple in-app notification, typically tied to an event.
/// Stored per user and queryable per user.

public sealed class Notification
{
    public required int Id { get; init; }

    public required int UserId { get; init; }

    public required int EventId { get; init; }

    /// <summary>
    /// A simple string category (e.g. "EventReminder").
    /// </summary>
    public required string Type { get; init; }

    public required string Message { get; init; }

    public NotificationState State { get; set; } = NotificationState.Unread;

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
