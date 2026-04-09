using BankApp.Contracts.Entities;
using BankApp.Server.DataAccess.Interfaces;
using Dapper;
using ErrorOr;

namespace BankApp.Server.DataAccess.Implementations;

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

    private readonly AppDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserDataAccess"/> class.
    /// </summary>
    /// <param name="dbContext">The database context used for executing queries.</param>
    public UserDataAccess(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public User? FindByEmail(string email)
    {
        const string query = $"{SelectAllColumns} WHERE Email = @Email";
        return this.dbContext.GetConnection().QueryFirstOrDefault<User>(query, new { Email = email });
    }

    /// <inheritdoc />
    public User? FindById(int id)
    {
        const string query = $"{SelectAllColumns} WHERE Id = @Id";
        return this.dbContext.GetConnection().QueryFirstOrDefault<User>(query, new { Id = id });
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

        var rows = this.dbContext.GetConnection().Execute(sql, user);
        return rows > 0 ? Result.Success : Error.Failure(description: "Failed to create user.");
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

        var rows = this.dbContext.GetConnection().Execute(sql, user);
        return rows > 0 ? Result.Success : Error.Failure(description: "Failed to update user.");
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

        var rows = this.dbContext.GetConnection().Execute(sql, new { UserId = userId, PasswordHash = newPasswordHash });
        return rows > 0 ? Result.Success : Error.Failure(description: "Failed to update password.");
    }

    /// <inheritdoc />
    public void IncrementFailedAttempts(int userId)
    {
        const string sql = "UPDATE [User] SET FailedLoginAttempts = FailedLoginAttempts + 1 WHERE Id = @UserId";
        this.dbContext.GetConnection().Execute(sql, new { UserId = userId });
    }

    /// <inheritdoc />
    public void ResetFailedAttempts(int userId)
    {
        const string sql = "UPDATE [User] SET FailedLoginAttempts = 0 WHERE Id = @UserId";
        this.dbContext.GetConnection().Execute(sql, new { UserId = userId });
    }

    /// <inheritdoc />
    public void LockAccount(int userId, DateTime lockoutEnd)
    {
        const string sql = "UPDATE [User] SET IsLocked = 1, LockoutEnd = @LockoutEnd WHERE Id = @UserId";
        this.dbContext.GetConnection().Execute(sql, new { UserId = userId, LockoutEnd = lockoutEnd });
    }
}
