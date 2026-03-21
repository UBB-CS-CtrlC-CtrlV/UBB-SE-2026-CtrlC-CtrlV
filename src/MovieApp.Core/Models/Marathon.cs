
namespace MovieApp.Core.Models;
public sealed class Marathon
{
    public required int Id { get; init; }
    public required string Title { get; init; }
    public string? Description { get; set; }
    public string? PosterUrl { get; set; }
    public string? Theme { get; set; }

    public int? PrerequisiteMarathonId { get; set; }
    public DateTime? LastFeaturedDate { get; set; }
    public bool IsActive { get; set; }
    public string? WeekScoping { get; set; }
}