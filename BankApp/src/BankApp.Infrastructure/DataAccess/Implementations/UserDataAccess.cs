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
               [Address], Nationality, PreferredLanguage, Is2FAEnabled, Preferred2FAMethod,
               IsLocked, LockoutEnd, FailedLoginAttempts, CreatedAt, UpdatedAt
        FROM [User]
        """;

    private readonly AppDbContext db;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserDataAccess"/> class.
    /// </summary>
    /// <param name="db">The database context used for executing queries.</param>
    public UserDataAccess(AppDbContext db)
    {
        this.db = db;
    }

    /// <inheritdoc />
    public ErrorOr<User> FindByEmail(string email)
    {
        const string query = $"{SelectAllColumns} WHERE Email = @Email";
        return this.db.Query(conn => conn.QueryFirstOrDefault<User>(query, new { Email = email }))
            .Then(user => user ?? (ErrorOr<User>)Error.NotFound(description: "User not found."));
    }

    /// <inheritdoc />
    public ErrorOr<User> FindById(int id)
    {
        const string query = $"{SelectAllColumns} WHERE Id = @Id";
        return this.db.Query(conn => conn.QueryFirstOrDefault<User>(query, new { Id = id }))
            .Then(user => user ?? (ErrorOr<User>)Error.NotFound(description: "User not found."));
    }

    /// <inheritdoc />
    public ErrorOr<Success> Create(User user)
    {
        const string sql = """
            INSERT INTO [User] (Email, PasswordHash, FullName, PhoneNumber, DateOfBirth,
                [Address], Nationality, PreferredLanguage, Is2FAEnabled, Preferred2FAMethod)
            VALUES (@Email, @PasswordHash, @FullName, @PhoneNumber, @DateOfBirth,
                @Address, @Nationality, @PreferredLanguage, @Is2FAEnabled, @Preferred2FAMethod)
            """;

        return this.db.Query(conn => conn.Execute(sql, user))
            .Then(rows => rows > 0 ? Result.Success : (ErrorOr<Success>)Error.Failure(description: "Failed to create user."));
    }

    /// <inheritdoc />
    public ErrorOr<Success> Update(User user)
    {
        const string sql = """
            UPDATE [User]
            SET Email              = @Email,
                FullName           = @FullName,
                PhoneNumber        = @PhoneNumber,
                DateOfBirth        = @DateOfBirth,
                [Address]          = @Address,
                Nationality        = @Nationality,
                PreferredLanguage  = @PreferredLanguage,
                Is2FAEnabled       = @Is2FAEnabled,
                Preferred2FAMethod = @Preferred2FAMethod,
                UpdatedAt          = GETUTCDATE()
            WHERE Id = @Id
            """;

        return this.db.Query(conn => conn.Execute(sql, user))
            .Then(rows => rows > 0 ? Result.Success : (ErrorOr<Success>)Error.Failure(description: "Failed to update user."));
    }

    /// <inheritdoc />
    public ErrorOr<Success> UpdatePassword(int userId, string newPasswordHash)
    {
        const string sql = """
            UPDATE [User]
            SET PasswordHash = @PasswordHash,
                UpdatedAt    = GETUTCDATE()
            WHERE Id = @UserId
            """;

        return this.db.Query(conn => conn.Execute(sql, new { UserId = userId, PasswordHash = newPasswordHash }))
            .Then(rows => rows > 0 ? Result.Success : (ErrorOr<Success>)Error.Failure(description: "Failed to update password."));
    }

    /// <inheritdoc />
    public ErrorOr<Success> IncrementFailedAttempts(int userId)
    {
        const string sql = "UPDATE [User] SET FailedLoginAttempts = FailedLoginAttempts + 1 WHERE Id = @UserId";
        return this.db.Query(conn => conn.Execute(sql, new { UserId = userId }))
            .Then(_ => (ErrorOr<Success>)Result.Success);
    }

    /// <inheritdoc />
    public ErrorOr<Success> ResetFailedAttempts(int userId)
    {
        const string sql = "UPDATE [User] SET FailedLoginAttempts = 0 WHERE Id = @UserId";
        return this.db.Query(conn => conn.Execute(sql, new { UserId = userId }))
            .Then(_ => (ErrorOr<Success>)Result.Success);
    }

    /// <inheritdoc />
    public ErrorOr<Success> LockAccount(int userId, DateTime lockoutEnd)
    {
        const string sql = "UPDATE [User] SET IsLocked = 1, LockoutEnd = @LockoutEnd WHERE Id = @UserId";
        return this.db.Query(conn => conn.Execute(sql, new { UserId = userId, LockoutEnd = lockoutEnd }))
            .Then(_ => (ErrorOr<Success>)Result.Success);
    }
}
