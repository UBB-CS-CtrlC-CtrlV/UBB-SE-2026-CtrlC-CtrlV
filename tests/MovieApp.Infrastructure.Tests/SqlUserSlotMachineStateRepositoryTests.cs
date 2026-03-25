using Xunit;

namespace MovieApp.Infrastructure.Tests;

public sealed class SqlUserSlotMachineStateRepositoryTests
{
    [Fact]
    public void Interface_DefinesExpectedMethods()
    {
        var file = ReadRepoFile("src", "MovieApp.Core", "Repositories", "IUserSlotMachineStateRepository.cs");

        Assert.Contains("Task<UserSpinData?> GetByUserIdAsync(int userId", file);
        Assert.Contains("Task CreateAsync(UserSpinData state", file);
        Assert.Contains("Task UpdateAsync(UserSpinData state", file);
    }

    [Fact]
    public void Repository_UsesDatabaseOptionsLikeOtherSqlRepositories()
    {
        var repositoryFile = ReadRepoFile("src", "MovieApp.Infrastructure", "SqlUserSlotMachineStateRepository.cs");

        Assert.Contains("public SqlUserSlotMachineStateRepository(DatabaseOptions databaseOptions)", repositoryFile);
        Assert.Contains("ArgumentNullException.ThrowIfNull(databaseOptions);", repositoryFile);
        Assert.Contains("_connectionString = databaseOptions.ConnectionString", repositoryFile);
        Assert.Contains("await using var connection = new SqlConnection(_connectionString);", repositoryFile);
        Assert.Contains("await using var command = new SqlCommand(", repositoryFile);
        Assert.DoesNotContain("public SqlUserSlotMachineStateRepository(string connectionString)", repositoryFile);
    }

    [Fact]
    public void Repository_ContainsUserSpinsQuery()
    {
        var repositoryFile = ReadRepoFile("src", "MovieApp.Infrastructure", "SqlUserSlotMachineStateRepository.cs");
        Assert.Contains("FROM dbo.UserSpins", repositoryFile);
        Assert.Contains("WHERE UserId = @userId", repositoryFile);
    }

    [Fact]
    public void UserSpins_Model_Exists()
    {
        var file = ReadRepoFile("src", "MovieApp.Core", "Models", "UserSpins.cs");
        Assert.Contains("public sealed class UserSpinData", file);
        Assert.Contains("DailySpinsRemaining", file);
        Assert.Contains("BonusSpins", file);
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
