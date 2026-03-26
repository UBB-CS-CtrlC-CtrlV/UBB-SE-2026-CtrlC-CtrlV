namespace MovieApp.Infrastructure;

/// <summary>
/// Centralizes the trivia-reward queries used by <see cref="SqlTriviaRewardRepository"/>.
/// </summary>
public static class TriviaRewardSqlQueries
{
    public const string Insert = """
        INSERT INTO dbo.TriviaRewards (UserId)
        VALUES (@userId);
        """;

    public const string SelectUnredeemedByUser = """
        SELECT Id, UserId, IsRedeemed, CreatedAt
        FROM dbo.TriviaRewards
        WHERE UserId = @userId
          AND IsRedeemed = 0;
        """;
}