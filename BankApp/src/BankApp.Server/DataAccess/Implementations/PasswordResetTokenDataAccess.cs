using BankApp.Contracts.Entities;
using BankApp.Server.DataAccess.Interfaces;
using Dapper;
using ErrorOr;

namespace BankApp.Server.DataAccess.Implementations;

/// <summary>
/// Provides SQL Server data access for password reset token records.
/// </summary>
public class PasswordResetTokenDataAccess : IPasswordResetTokenDataAccess
{
    private readonly AppDbContext db;

    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordResetTokenDataAccess"/> class.
    /// </summary>
    /// <param name="db">The database context used for executing queries.</param>
    public PasswordResetTokenDataAccess(AppDbContext db)
    {
        this.db = db;
    }

    /// <inheritdoc />
    public ErrorOr<PasswordResetToken> Create(int userId, string tokenHash, DateTime expiresAt)
    {
        const string sql = """
            INSERT INTO PasswordResetToken (UserId, TokenHash, ExpiresAt)
            OUTPUT INSERTED.Id, INSERTED.UserId, INSERTED.TokenHash,
                   INSERTED.ExpiresAt, INSERTED.UsedAt, INSERTED.CreatedAt
            VALUES (@UserId, @TokenHash, @ExpiresAt)
            """;

        return this.db.Query(conn => conn.QueryFirstOrDefault<PasswordResetToken>(sql, new
        {
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
        })).Then(token => token ?? (ErrorOr<PasswordResetToken>)Error.Failure(description: "Failed to create password reset token."));
    }

    /// <inheritdoc />
    public ErrorOr<PasswordResetToken> FindByToken(string tokenHash)
    {
        const string sql = """
            SELECT Id, UserId, TokenHash, ExpiresAt, UsedAt, CreatedAt
            FROM PasswordResetToken
            WHERE TokenHash = @TokenHash
            """;

        return this.db.Query(conn => conn.QueryFirstOrDefault<PasswordResetToken>(sql, new { TokenHash = tokenHash }))
            .Then(token => token ?? (ErrorOr<PasswordResetToken>)Error.NotFound(description: "Password reset token not found."));
    }

    /// <inheritdoc />
    public ErrorOr<Success> MarkAsUsed(int tokenId)
    {
        const string sql = "UPDATE PasswordResetToken SET UsedAt = GETUTCDATE() WHERE Id = @Id";
        return this.db.Query(conn => conn.Execute(sql, new { Id = tokenId }))
            .Then(_ => (ErrorOr<Success>)Result.Success);
    }

    /// <inheritdoc />
    public ErrorOr<Success> DeleteExpired()
    {
        const string sql = "DELETE FROM PasswordResetToken WHERE ExpiresAt < GETUTCDATE()";
        return this.db.Query(conn => conn.Execute(sql))
            .Then(_ => (ErrorOr<Success>)Result.Success);
    }
}
