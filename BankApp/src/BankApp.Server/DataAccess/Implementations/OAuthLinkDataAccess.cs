using BankApp.Contracts.Entities;
using BankApp.Server.DataAccess.Interfaces;
using Dapper;
using ErrorOr;

namespace BankApp.Server.DataAccess.Implementations;

/// <summary>
/// Provides SQL Server data access for OAuth provider link records.
/// </summary>
public class OAuthLinkDataAccess : IOAuthLinkDataAccess
{
    private const string SelectAllColumns = """
        SELECT Id, UserId, Provider, ProviderUserId, ProviderEmail, LinkedAt
        FROM OAuthLink
        """;

    private readonly AppDbContext context;

    /// <summary>
    /// Initializes a new instance of the <see cref="OAuthLinkDataAccess"/> class.
    /// </summary>
    /// <param name="context">The database context used for executing queries.</param>
    public OAuthLinkDataAccess(AppDbContext context)
    {
        this.context = context;
    }

    /// <inheritdoc />
    public ErrorOr<Success> Create(int userId, string provider, string providerUserId, string? providerEmail)
    {
        const string sql = """
            INSERT INTO OAuthLink (UserId, Provider, ProviderUserId, ProviderEmail)
            VALUES (@UserId, @Provider, @ProviderUserId, @ProviderEmail)
            """;

        var rows = this.context.GetConnection().Execute(sql, new
        {
            UserId = userId,
            Provider = provider,
            ProviderUserId = providerUserId,
            ProviderEmail = providerEmail,
        });
        return rows > 0 ? Result.Success : Error.Failure(description: "Failed to create OAuth link.");
    }

    /// <inheritdoc />
    public void Delete(int id)
    {
        const string sql = "DELETE FROM OAuthLink WHERE Id = @Id";
        this.context.GetConnection().Execute(sql, new { Id = id });
    }

    /// <inheritdoc />
    public OAuthLink? FindByProvider(string provider, string providerUserId)
    {
        const string query = $"{SelectAllColumns} WHERE Provider = @Provider AND ProviderUserId = @ProviderUserId";
        return this.context.GetConnection().QueryFirstOrDefault<OAuthLink>(query, new { Provider = provider, ProviderUserId = providerUserId });
    }

    /// <inheritdoc />
    public List<OAuthLink> FindByUserId(int userId)
    {
        const string query = $"{SelectAllColumns} WHERE UserId = @UserId";
        return this.context.GetConnection().Query<OAuthLink>(query, new { UserId = userId }).AsList();
    }
}
