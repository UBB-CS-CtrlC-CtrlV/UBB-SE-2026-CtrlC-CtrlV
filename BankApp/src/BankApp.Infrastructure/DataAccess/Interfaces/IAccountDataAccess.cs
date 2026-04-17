using BankApp.Domain.Entities;
using ErrorOr;

namespace BankApp.Infrastructure.DataAccess.Interfaces;

/// <summary>
/// Defines data access operations for bank accounts.
/// </summary>
public interface IAccountDataAccess
{
    /// <summary>Finds all accounts belonging to the specified user.</summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <returns>A list of accounts owned by the user, or an error if the operation failed.</returns>
    ErrorOr<List<Account>> FindByUserId(int userId);

    /// <summary>Finds an account by its unique identifier.</summary>
    /// <param name="id">The account identifier.</param>
    /// <returns>The matching <see cref="Account"/>, or <see cref="Error.NotFound"/> if not found.</returns>
    ErrorOr<Account> FindById(int id);
}
