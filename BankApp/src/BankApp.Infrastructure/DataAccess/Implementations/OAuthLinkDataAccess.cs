using BankApp.Domain.Entities;
using BankApp.Infrastructure.DataAccess.Interfaces;
using Dapper;
using ErrorOr;

namespace BankApp.Infrastructure.DataAccess.Implementations;

/// <summary>
/// Provides SQL Server data access for OAuth provider link records.
/// </summary>
public class OAuthLinkDataAccess : IOAuthLinkDataAccess
{
    private const string SelectAllColumns = """
        SELECT Id, UserId, Provider, ProviderUserId, ProviderEmail, LinkedAt
        FROM OAuthLink
        """;

    private readonly AppDbContext db;

    /// <summary>
    /// Initializes a new instance of the <see cref="OAuthLinkDataAccess"/> class.
    /// </summary>
    /// <param name="db">The database context used for executing queries.</param>
    public OAuthLinkDataAccess(AppDbContext db)
    {
        this.db = db;
    }

    /// <inheritdoc />
    public ErrorOr<Success> Create(int userId, string provider, string providerUserId, string? providerEmail)
    {
        const string sql = """
            INSERT INTO OAuthLink (UserId, Provider, ProviderUserId, ProviderEmail)
            VALUES (@UserId, @Provider, @ProviderUserId, @ProviderEmail)
            """;

        return this.db.Query(conn => conn.Execute(sql, new
        {
            UserId = userId,
            Provider = provider,
            ProviderUserId = providerUserId,
            ProviderEmail = providerEmail,
        })).Then(rows => rows > 0 ? Result.Success : (ErrorOr<Success>)Error.Failure(description: "Failed to create OAuth link."));
    }

    /// <inheritdoc />
    public ErrorOr<Success> Delete(int id)
    {
        const string sql = "DELETE FROM OAuthLink WHERE Id = @Id";
        return this.db.Query(conn => conn.Execute(sql, new { Id = id }))
            .Then(_ => (ErrorOr<Success>)Result.Success);
    }

    /// <inheritdoc />
    public ErrorOr<OAuthLink> FindByProvider(string provider, string providerUserId)
    {
        const string query = $"{SelectAllColumns} WHERE Provider = @Provider AND ProviderUserId = @ProviderUserId";
        return this.db.Query(conn => conn.QueryFirstOrDefault<OAuthLink>(query, new { Provider = provider, ProviderUserId = providerUserId }))
            .Then(link => link ?? (ErrorOr<OAuthLink>)Error.NotFound(description: "OAuth link not found."));
    }

    /// <inheritdoc />
    public ErrorOr<List<OAuthLink>> FindByUserId(int userId)
    {
        const string query = $"{SelectAllColumns} WHERE UserId = @UserId";
        return this.db.Query(conn => conn.Query<OAuthLink>(query, new { UserId = userId }).AsList());
    }
}
