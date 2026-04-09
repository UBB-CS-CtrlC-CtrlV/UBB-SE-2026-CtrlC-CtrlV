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
    private readonly AppDbContext context;

    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordResetTokenDataAccess"/> class.
    /// </summary>
    /// <param name="context">The database context used for executing queries.</param>
    public PasswordResetTokenDataAccess(AppDbContext context)
    {
        this.context = context;
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

        var token = this.context.GetConnection().QueryFirstOrDefault<PasswordResetToken>(sql, new
        {
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
        });

        return token is null
            ? Error.Failure(description: "Failed to create password reset token.")
            : token;
    }

    /// <inheritdoc />
    public PasswordResetToken? FindByToken(string tokenHash)
    {
        const string sql = """
            SELECT Id, UserId, TokenHash, ExpiresAt, UsedAt, CreatedAt
            FROM PasswordResetToken
            WHERE TokenHash = @TokenHash
            """;

        return this.context.GetConnection().QueryFirstOrDefault<PasswordResetToken>(sql, new { TokenHash = tokenHash });
    }

    /// <inheritdoc />
    public void MarkAsUsed(int tokenId)
    {
        const string sql = "UPDATE PasswordResetToken SET UsedAt = GETUTCDATE() WHERE Id = @Id";
        this.context.GetConnection().Execute(sql, new { Id = tokenId });
    }

    /// <inheritdoc />
    public void DeleteExpired()
    {
        const string sql = "DELETE FROM PasswordResetToken WHERE ExpiresAt < GETUTCDATE()";
        this.context.GetConnection().Execute(sql);
    }
}
