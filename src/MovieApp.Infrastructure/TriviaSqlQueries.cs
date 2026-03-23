namespace MovieApp.Infrastructure;

/// <summary>
/// Centralizes the trivia-table projections used by <see cref="SqlTriviaRepository"/>.
/// </summary>
public static class TriviaSqlQueries
{
    /// <summary>
    /// The column order here must stay aligned with <see cref="SqlTriviaRepository.MapTriviaQuestion"/>.
    /// </summary>
    public const string Projection = """
        Id, QuestionText, Category, OptionA, OptionB, OptionC, OptionD, CorrectOption, MovieId
        """;

    public const string SelectByCategory = $$"""
        SELECT {{Projection}}
        FROM dbo.TriviaQuestions
        WHERE Category = @category;
        """;
}