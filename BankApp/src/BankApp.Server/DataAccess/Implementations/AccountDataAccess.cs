using System.Data;
using BankApp.Contracts.Entities;
using BankApp.Server.DataAccess.Interfaces;
using Dapper;
using ErrorOr;

namespace BankApp.Server.DataAccess.Implementations;

/// <summary>
/// Provides SQL Server data access for bank account records.
/// </summary>
public class AccountDataAccess : IAccountDataAccess
{
    private readonly AppDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountDataAccess"/> class.
    /// </summary>
    /// <param name="dbContext">The database context used for executing queries.</param>
    public AccountDataAccess(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public ErrorOr<Account> FindById(int id)
    {
        const string query = """
            SELECT Id, UserId, AccountName, IBAN, Currency, Balance,
                   AccountType, Status, CreatedAt
            FROM Account
            WHERE Id = @Id
            """;

        var account = this.dbContext.GetConnection()
          .QueryFirstOrDefault<Account>(query, new { Id = id });

        return account is null
          ? Error.NotFound(description: "Account not found.")
          : account;
    }

    /// <inheritdoc />
    public List<Account> FindByUserId(int userId)
    {
        const string query = """
            SELECT 
                Id, UserId, AccountName, 
                IBAN, Currency, Balance, 
                AccountType, Status, CreatedAt 
            FROM Account WHERE UserId = @UserId
            """;

        return this.dbContext.GetConnection().Query<Account>(query, new { UserId = userId }).AsList();
    }
}