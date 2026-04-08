namespace BankApp.Contracts.Entities;

/// <summary>
/// Represents a notification sent to a user.
/// </summary>
public class Notification
{
    /// <summary>
    /// Gets or sets the unique identifier for the notification.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user this notification belongs to.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the title of the notification.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the message body of the notification.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of the notification.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the delivery channel used for the notification.
    /// </summary>
    public string Channel { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the notification has been read.
    /// </summary>
    public bool IsRead { get; set; }

    /// <summary>
    /// Gets or sets the type of entity related to this notification.
    /// </summary>
    public string? RelatedEntityType { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the entity related to this notification.
    /// </summary>
    public int? RelatedEntityId { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the notification was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}