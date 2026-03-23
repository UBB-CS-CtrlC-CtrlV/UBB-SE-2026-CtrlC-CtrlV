using Microsoft.Data.SqlClient;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Infrastructure;

public sealed class SqlTriviaRepository(DatabaseOptions databaseOptions) : ITriviaRepository
{
    private readonly string _connectionString = databaseOptions.ConnectionString;

    public async Task<IEnumerable<TriviaQuestion>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        var questions = new List<TriviaQuestion>();

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(TriviaSqlQueries.SelectByCategory, connection);
        command.Parameters.AddWithValue("@category", category);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            questions.Add(MapTriviaQuestion(reader));
        }

        return questions;
    }

    private static TriviaQuestion MapTriviaQuestion(SqlDataReader reader)
    {
        return new TriviaQuestion
        {
            Id = reader.GetInt32(0),
            QuestionText = reader.GetString(1),
            Category = reader.GetString(2),
            OptionA = reader.GetString(3),
            OptionB = reader.GetString(4),
            OptionC = reader.GetString(5),
            OptionD = reader.GetString(6),
            CorrectOption = reader.GetString(7)[0],
            MovieId = reader.IsDBNull(8) ? null : reader.GetInt32(8)
        };
    }
}