namespace MovieApp.Infrastructure;

/// <summary>
/// Centralizes the event-table projections used by <see cref="SqlEventRepository"/>.
/// </summary>
public static class EventSqlQueries
{
    /// <summary>
    /// The column order here must stay aligned with <see cref="SqlEventRepository.MapEvent"/>.
    /// </summary>
    public const string Projection = """
        Id, Title, Description, PosterUrl, EventDateTime, LocationReference,
        TicketPrice, HistoricalRating, EventType, MaxCapacity, CurrentEnrollment, CreatorUserId
        """;

    public const string SelectAll = $$"""
        SELECT {{Projection}}
        FROM dbo.Events;
        """;

    public const string SelectByType = $$"""
        SELECT {{Projection}}
        FROM dbo.Events
        WHERE EventType = @eventType;
        """;

    public const string SelectById = $$"""
        SELECT {{Projection}}
        FROM dbo.Events
        WHERE Id = @id;
        """;

    /// <summary>
    /// Insert statement used when persisting new events. Keep the column list aligned
    /// with the parameters assigned in <see cref="SqlEventRepository.AddAsync"/>.
    /// </summary>
    public const string Insert = """
        INSERT INTO dbo.Events (Title, Description, PosterUrl, EventDateTime, LocationReference,
                               TicketPrice, EventType, HistoricalRating, MaxCapacity, CurrentEnrollment, CreatorUserId)
        VALUES (@title, @description, @posterUrl, @eventDateTime, @locationReference,
                @ticketPrice, @eventType, @historicalRating, @maxCapacity, @currentEnrollment, @creatorUserId);
        SELECT CAST(SCOPE_IDENTITY() as int);
        """;
}
