using BankApp.Domain.Entities;
using BankApp.Infrastructure.DataAccess.Interfaces;
using Dapper;
using ErrorOr;

namespace BankApp.Infrastructure.DataAccess.Implementations;

/// <summary>
/// Provides SQL Server data access for user account records.
/// </summary>
public class UserDataAccess : IUserDataAccess
{
    private const string SelectAllColumns = """
        SELECT Id, Email, PasswordHash, FullName, PhoneNumber, DateOfBirth,
               [Address], Nationality, PreferredLanguage,
               Is2FAEnabled AS Is2FactorAuthenticationEnabled, Preferred2FAMethod,
               IsLocked, LockoutEnd, FailedLoginAttempts, CreatedAt, UpdatedAt
        FROM [User]
        """;

    private readonly AppDatabaseContext databaseContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserDataAccess"/> class.
    /// </summary>
    /// <param name="databaseContext">The database context used for executing queries.</param>
    public UserDataAccess(AppDatabaseContext databaseContext)
    {
        this.databaseContext = databaseContext;
    }

    /// <inheritdoc />
    public ErrorOr<User> FindByEmail(string email)
    {
        const string query = $"{SelectAllColumns} WHERE Email = @Email";
        return this.databaseContext.Query(connection => connection.QueryFirstOrDefault<User>(query, new { Email = email }))
            .Then(user => user ?? (ErrorOr<User>)Error.NotFound(description: "User not found."));
    }

    /// <inheritdoc />
    public ErrorOr<User> FindById(int id)
    {
        const string query = $"{SelectAllColumns} WHERE Id = @Id";
        return this.databaseContext.Query(connection => connection.QueryFirstOrDefault<User>(query, new { Id = id }))
            .Then(user => user ?? (ErrorOr<User>)Error.NotFound(description: "User not found."));
    }

    /// <inheritdoc />
    public ErrorOr<Success> Create(User user)
    {
        const string databaseCommandText = """
            INSERT INTO [User] (Email, PasswordHash, FullName, PhoneNumber, DateOfBirth,
                [Address], Nationality, PreferredLanguage, Is2FAEnabled, Preferred2FAMethod)
            VALUES (@Email, @PasswordHash, @FullName, @PhoneNumber, @DateOfBirth,
                @Address, @Nationality, @PreferredLanguage, @Is2FactorAuthenticationEnabled, @Preferred2FAMethod)
            """;

        return this.databaseContext.Query(connection => connection.Execute(databaseCommandText, user))
            .Then(rows => rows > default(int) ? Result.Success : (ErrorOr<Success>)Error.Failure(description: "Failed to create user."));
    }

    /// <inheritdoc />
    public ErrorOr<Success> Update(User user)
    {
        const string databaseCommandText = """
            UPDATE [User]
            SET Email              = @Email,
                FullName           = @FullName,
                PhoneNumber        = @PhoneNumber,
                DateOfBirth        = @DateOfBirth,
                [Address]          = @Address,
                Nationality        = @Nationality,
                PreferredLanguage  = @PreferredLanguage,
                Is2FAEnabled       = @Is2FactorAuthenticationEnabled,
                Preferred2FAMethod = @Preferred2FAMethod,
                UpdatedAt          = GETUTCDATE()
            WHERE Id = @Id
            """;

        return this.databaseContext.Query(connection => connection.Execute(databaseCommandText, user))
            .Then(rows => rows > default(int) ? Result.Success : (ErrorOr<Success>)Error.Failure(description: "Failed to update user."));
    }

    /// <inheritdoc />
    public ErrorOr<Success> UpdatePassword(int userId, string newPasswordHash)
    {
        const string databaseCommandText = """
            UPDATE [User]
            SET PasswordHash = @PasswordHash,
                UpdatedAt    = GETUTCDATE()
            WHERE Id = @UserId
            """;

        return this.databaseContext.Query(connection => connection.Execute(databaseCommandText, new { UserId = userId, PasswordHash = newPasswordHash }))
            .Then(rows => rows > default(int) ? Result.Success : (ErrorOr<Success>)Error.Failure(description: "Failed to update password."));
    }

    /// <inheritdoc />
    public ErrorOr<Success> IncrementFailedAttempts(int userId)
    {
        const string databaseCommandText = "UPDATE [User] SET FailedLoginAttempts = FailedLoginAttempts + 1 WHERE Id = @UserId";
        return this.databaseContext.Query(connection => connection.Execute(databaseCommandText, new { UserId = userId }))
            .Then(_ => (ErrorOr<Success>)Result.Success);
    }

    /// <inheritdoc />
    public ErrorOr<Success> ResetFailedAttempts(int userId)
    {
        const string databaseCommandText = "UPDATE [User] SET FailedLoginAttempts = default WHERE Id = @UserId";
        return this.databaseContext.Query(connection => connection.Execute(databaseCommandText, new { UserId = userId }))
            .Then(_ => (ErrorOr<Success>)Result.Success);
    }

    /// <inheritdoc />
    public ErrorOr<Success> LockAccount(int userId, DateTime lockoutEnd)
    {
        const string databaseCommandText = "UPDATE [User] SET IsLocked = 1, LockoutEnd = @LockoutEnd WHERE Id = @UserId";
        return this.databaseContext.Query(connection => connection.Execute(databaseCommandText, new { UserId = userId, LockoutEnd = lockoutEnd }))
            .Then(_ => (ErrorOr<Success>)Result.Success);
    }
}
