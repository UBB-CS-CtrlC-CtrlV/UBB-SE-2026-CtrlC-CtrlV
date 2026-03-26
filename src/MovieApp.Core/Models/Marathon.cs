
namespace MovieApp.Core.Models;
/// <summary>
/// Represents a themed movie marathon definition for a given weekly cycle.
/// </summary>
public sealed class Marathon
{
    /// <summary>
    /// Gets the marathon identifier.
    /// </summary>
    public required int Id { get; init; }
    /// <summary>
    /// Gets the display title of the marathon.
    /// </summary>
    public required string Title { get; init; }
    /// <summary>
    /// Gets or sets the descriptive copy shown to the user.
    /// </summary>
    public string? Description { get; set; }
    /// <summary>
    /// Gets or sets the poster artwork URL.
    /// </summary>
    public string? PosterUrl { get; set; }
    /// <summary>
    /// Gets or sets the theme label used for grouping and presentation.
    /// </summary>
    public string? Theme { get; set; }

    /// <summary>
    /// Gets or sets the prerequisite marathon identifier for locked elite marathons.
    /// </summary>
    public int? PrerequisiteMarathonId { get; set; }
    /// <summary>
    /// Gets or sets the last date the marathon was featured.
    /// </summary>
    public DateTime? LastFeaturedDate { get; set; }
    /// <summary>
    /// Gets or sets whether the marathon is currently active.
    /// </summary>
    public bool IsActive { get; set; }
    /// <summary>
    /// Gets or sets the week-scoping token used to isolate weekly marathon sets.
    /// </summary>
    public string? WeekScoping { get; set; }
}
