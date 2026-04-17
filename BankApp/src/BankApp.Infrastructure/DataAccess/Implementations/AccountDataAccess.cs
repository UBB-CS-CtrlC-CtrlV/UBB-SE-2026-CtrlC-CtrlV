using BankApp.Domain.Entities;
using BankApp.Infrastructure.DataAccess.Interfaces;
using Dapper;
using ErrorOr;

namespace BankApp.Infrastructure.DataAccess.Implementations;

/// <summary>
/// Provides SQL Server data access for bank account records.
/// </summary>
public class AccountDataAccess : IAccountDataAccess
{
    private const string SelectAllColumns = """
        SELECT Id, UserId, AccountName, IBAN, Currency, Balance,
               AccountType, Status, CreatedAt
        FROM Account
        """;

    private readonly AppDatabaseContext databaseContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountDataAccess"/> class.
    /// </summary>
    /// <param name="databaseContext">The database context used for executing queries.</param>
    public AccountDataAccess(AppDatabaseContext databaseContext)
    {
        this.databaseContext = databaseContext;
    }

    /// <inheritdoc />
    public ErrorOr<Account> FindById(int id)
    {
        const string query = $"{SelectAllColumns} WHERE Id = @Id";
        return this.databaseContext.Query(connection => connection.QueryFirstOrDefault<Account>(query, new { Id = id }))
            .Then(account => account ?? (ErrorOr<Account>)Error.NotFound(description: "Account not found."));
    }

    /// <inheritdoc />
    public ErrorOr<List<Account>> FindByUserId(int userId)
    {
        const string query = $"{SelectAllColumns} WHERE UserId = @UserId";
        return this.databaseContext.Query(connection => connection.Query<Account>(query, new { UserId = userId }).AsList());
    }
}
