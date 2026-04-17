using BankApp.Domain.Entities;
using BankApp.Infrastructure.DataAccess.Interfaces;
using Dapper;
using ErrorOr;

namespace BankApp.Infrastructure.DataAccess.Implementations;

/// <summary>
/// Provides SQL Server data access for password reset token records.
/// </summary>
public class PasswordResetTokenDataAccess : IPasswordResetTokenDataAccess
{
    private readonly AppDatabaseContext databaseContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordResetTokenDataAccess"/> class.
    /// </summary>
    /// <param name="databaseContext">The database context used for executing queries.</param>
    public PasswordResetTokenDataAccess(AppDatabaseContext databaseContext)
    {
        this.databaseContext = databaseContext;
    }

    /// <inheritdoc />
    public ErrorOr<PasswordResetToken> Create(int userId, string tokenHash, DateTime expiresAt)
    {
        const string databaseCommandText = """
            INSERT INTO PasswordResetToken (UserId, TokenHash, ExpiresAt)
            OUTPUT INSERTED.Id, INSERTED.UserId, INSERTED.TokenHash,
                   INSERTED.ExpiresAt, INSERTED.UsedAt, INSERTED.CreatedAt
            VALUES (@UserId, @TokenHash, @ExpiresAt)
            """;

        return this.databaseContext.Query(connection => connection.QueryFirstOrDefault<PasswordResetToken>(databaseCommandText, new
        {
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
        })).Then(token => token ?? (ErrorOr<PasswordResetToken>)Error.Failure(description: "Failed to create password reset token."));
    }

    /// <inheritdoc />
    public ErrorOr<PasswordResetToken> FindByToken(string tokenHash)
    {
        const string databaseCommandText = """
            SELECT Id, UserId, TokenHash, ExpiresAt, UsedAt, CreatedAt
            FROM PasswordResetToken
            WHERE TokenHash = @TokenHash
            """;

        return this.databaseContext.Query(connection => connection.QueryFirstOrDefault<PasswordResetToken>(databaseCommandText, new { TokenHash = tokenHash }))
            .Then(token => token ?? (ErrorOr<PasswordResetToken>)Error.NotFound(description: "Password reset token not found."));
    }

    /// <inheritdoc />
    public ErrorOr<Success> MarkAsUsed(int tokenId)
    {
        const string databaseCommandText = "UPDATE PasswordResetToken SET UsedAt = GETUTCDATE() WHERE Id = @Id";
        return this.databaseContext.Query(connection => connection.Execute(databaseCommandText, new { Id = tokenId }))
            .Then(_ => (ErrorOr<Success>)Result.Success);
    }

    /// <inheritdoc />
    public ErrorOr<Success> DeleteExpired()
    {
        const string databaseCommandText = "DELETE FROM PasswordResetToken WHERE ExpiresAt < GETUTCDATE()";
        return this.databaseContext.Query(connection => connection.Execute(databaseCommandText))
            .Then(_ => (ErrorOr<Success>)Result.Success);
    }
}
