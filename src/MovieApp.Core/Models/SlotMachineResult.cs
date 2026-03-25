using System.Collections.Generic;
using MovieApp.Core.Models.Movie;
using MovieModel = MovieApp.Core.Models.Movie.Movie;

namespace MovieApp.Core.Models;

/// <summary>
/// Represents the results of a single slot-machine spin.
/// </summary>
public sealed class SlotMachineResult
{
    /// <summary>
    /// Selected genre reel.
    /// </summary>
    public Genre Genre { get; set; } = new Genre();

    /// <summary>
    /// Selected actor reel.
    /// </summary>
    public Actor Actor { get; set; } = new Actor();

    /// <summary>
    /// Selected director reel.
    /// </summary>
    public Director Director { get; set; } = new Director();

    /// <summary>
    /// Events matching the combination.
    /// </summary>
    public List<Event> MatchingEvents { get; set; } = new List<Event>();

    /// <summary>
    /// Movie triggering jackpot (nullable).
    /// </summary>
    public MovieModel? JackpotMovie { get; set; }

    /// <summary>
    /// Indicates jackpot discount applied.
    /// </summary>
    public bool JackpotDiscountApplied { get; set; }

    /// <summary>
    /// Discount applied if jackpot (0 if none).
    /// </summary>
    public int DiscountPercentage { get; set; }
}
