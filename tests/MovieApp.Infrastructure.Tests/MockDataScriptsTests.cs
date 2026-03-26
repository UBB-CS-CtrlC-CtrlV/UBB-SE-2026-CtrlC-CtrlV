using Xunit;

namespace MovieApp.Infrastructure.Tests;

/// <summary>
/// Verifies that the optional mock-data SQL entrypoint stays separate from the core schema scripts
/// and continues to cover the demo tables the app reads at runtime.
/// </summary>
public sealed class MockDataScriptsTests
{
    [Fact]
    public void MockBootstrapScript_ReferencesAllMockDataBatches()
    {
        var bootstrapFile = ReadRepoFile("src", "MovieApp.Infrastructure", "Database", "MockData", "000-bootstrap-mock-data.sql");

        Assert.Contains(@":r .\001-seed-dummy-user.sql", bootstrapFile);
        Assert.Contains(@":r .\002-seed-base-events.sql", bootstrapFile);
        Assert.Contains(@":r .\003-seed-base-trivia-questions.sql", bootstrapFile);
        Assert.Contains(@":r .\004-seed-base-movies-and-cast.sql", bootstrapFile);
        Assert.Contains(@":r .\005-seed-base-user-spins.sql", bootstrapFile);
        Assert.Contains(@":r .\006-seed-base-marathons.sql", bootstrapFile);
        Assert.Contains(@":r .\007-seed-extra-users-and-events.sql", bootstrapFile);
        Assert.Contains(@":r .\008-seed-extra-catalog-and-trivia.sql", bootstrapFile);
        Assert.Contains(@":r .\009-seed-engagement-and-rewards.sql", bootstrapFile);
        Assert.Contains(@":r .\010-seed-screenings-and-marathons.sql", bootstrapFile);
    }

    [Fact]
    public void EngagementMockData_CoversRelationshipDrivenTables()
    {
        var engagementFile = ReadRepoFile("src", "MovieApp.Infrastructure", "Database", "MockData", "009-seed-engagement-and-rewards.sql");

        Assert.Contains("INSERT INTO dbo.UserSpins", engagementFile);
        Assert.Contains("INSERT INTO dbo.Participations", engagementFile);
        Assert.Contains("INSERT INTO dbo.FavoriteEvents", engagementFile);
        Assert.Contains("INSERT INTO dbo.Notifications", engagementFile);
        Assert.Contains("INSERT INTO dbo.UserMovieDiscounts", engagementFile);
        Assert.Contains("INSERT INTO dbo.AmbassadorProfile", engagementFile);
        Assert.Contains("INSERT INTO dbo.ReferralLog", engagementFile);
        Assert.Contains("INSERT INTO dbo.TriviaRewards", engagementFile);
    }

    [Fact]
    public void CatalogMockData_AddsMovieSpecificTriviaForMarathonScenarios()
    {
        var catalogFile = ReadRepoFile("src", "MovieApp.Infrastructure", "Database", "MockData", "008-seed-extra-catalog-and-trivia.sql");

        Assert.Contains("INSERT INTO dbo.Movies", catalogFile);
        Assert.Contains("INSERT INTO dbo.MovieGenres", catalogFile);
        Assert.Contains("INSERT INTO dbo.MovieActors", catalogFile);
        Assert.Contains("INSERT INTO dbo.MovieDirectors", catalogFile);
        Assert.Contains("INSERT INTO dbo.TriviaQuestions", catalogFile);
        Assert.Contains("MovieId", catalogFile);
        Assert.Contains("N'Inception'", catalogFile);
        Assert.Contains("N'La La Land'", catalogFile);
    }

    private static string ReadRepoFile(params string[] pathSegments)
    {
        var currentDirectory = new DirectoryInfo(AppContext.BaseDirectory);

        while (currentDirectory is not null
            && !File.Exists(Path.Combine(currentDirectory.FullName, "MovieApp.sln")))
        {
            currentDirectory = currentDirectory.Parent;
        }

        Assert.NotNull(currentDirectory);

        var filePath = Path.Combine([currentDirectory!.FullName, .. pathSegments]);
        return File.ReadAllText(filePath);
    }
}
