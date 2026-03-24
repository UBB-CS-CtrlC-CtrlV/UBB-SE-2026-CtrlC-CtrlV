namespace MovieApp.Ui.ViewModels.Events;

/// <summary>
/// Carries the information required to open a section-specific event page.
/// </summary>
public sealed class SectionNavigationContext
{
    /// <summary>
    /// Gets the title that should be displayed by the destination page.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Gets the normalized grouping value used to select the matching events.
    /// </summary>
    public required string GroupingValue { get; init; }
}
