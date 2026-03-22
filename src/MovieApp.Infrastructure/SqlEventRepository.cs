using Microsoft.Data.SqlClient;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Infrastructure;

public sealed class SqlEventRepository(DatabaseOptions databaseOptions) : IEventRepository
{
    private readonly string _connectionString = databaseOptions.ConnectionString;

    public async Task<IEnumerable<Event>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var events = new List<Event>();

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(EventSqlQueries.SelectAll, connection);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            events.Add(MapEvent(reader));
        }

        return events;
    }

    public async Task<IEnumerable<Event>> GetAllByTypeAsync(string eventType, CancellationToken cancellationToken = default)
    {
        var events = new List<Event>();

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(EventSqlQueries.SelectByType, connection);
        command.Parameters.AddWithValue("@eventType", eventType);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            events.Add(MapEvent(reader));
        }

        return events;
    }

    public async Task<int> AddAsync(Event @event, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(EventSqlQueries.Insert, connection);
        command.Parameters.AddWithValue("@title", @event.Title);
        command.Parameters.AddWithValue("@description", (object?)@event.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("@posterUrl", @event.PosterUrl);
        command.Parameters.AddWithValue("@eventDateTime", @event.EventDateTime);
        command.Parameters.AddWithValue("@locationReference", @event.LocationReference);
        command.Parameters.AddWithValue("@ticketPrice", @event.TicketPrice);
        command.Parameters.AddWithValue("@eventType", @event.EventType);
        command.Parameters.AddWithValue("@historicalRating", @event.HistoricalRating);
        command.Parameters.AddWithValue("@maxCapacity", @event.MaxCapacity);
        command.Parameters.AddWithValue("@currentEnrollment", @event.CurrentEnrollment);
        command.Parameters.AddWithValue("@creatorUserId", @event.CreatorUserId);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        if (result is null or DBNull)
        {
            throw new InvalidOperationException("Expected the event insert to return the new identity value.");
        }

        return Convert.ToInt32(result);
    }

    public async Task<Event?> FindByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(EventSqlQueries.SelectById, connection);
        command.Parameters.AddWithValue("@id", id);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return MapEvent(reader);
    }

    public async Task<bool> UpdateEnrollmentAsync(int eventId, int newCount, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE dbo.Events
            SET CurrentEnrollment = @newCount
            WHERE Id = @eventId;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@newCount", newCount);
        command.Parameters.AddWithValue("@eventId", eventId);

        var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
        return rowsAffected > 0;
    }

    /// <summary>
    /// Maps an event row using the column order defined in <see cref="EventSqlQueries.Projection"/>.
    /// </summary>
    private static Event MapEvent(SqlDataReader reader)
    {
        return new Event
        {
            Id = reader.GetInt32(0),
            Title = reader.GetString(1),
            Description = reader.IsDBNull(2) ? null : reader.GetString(2),
            PosterUrl = reader.GetString(3),
            EventDateTime = reader.GetDateTime(4),
            LocationReference = reader.GetString(5),
            TicketPrice = reader.GetDecimal(6),
            HistoricalRating = reader.GetDouble(7),
            EventType = reader.GetString(8),
            MaxCapacity = reader.GetInt32(9),
            CurrentEnrollment = reader.GetInt32(10),
            CreatorUserId = reader.GetInt32(11)
        };
    }
}
