using BankApp.Contracts.Entities;
using ErrorOr;

namespace BankApp.Server.DataAccess.Interfaces;

/// <summary>
/// Defines data access operations for user accounts.
/// </summary>
public interface IUserDataAccess
{
    /// <summary>
    /// Finds a user by their email address.
    /// </summary>
    /// <param name="email">The email address to search for.</param>
    /// <returns>The matching <see cref="User"/>, or <see langword="null"/> if not found.</returns>
    User? FindByEmail(string email);

    /// <summary>
    /// Finds a user by their unique identifier.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <returns>The matching <see cref="User"/>, or <see langword="null"/> if not found.</returns>
    User? FindById(int id);

    /// <summary>
    /// Creates a new user record.
    /// </summary>
    /// <param name="user">The user entity to create.</param>
    /// <returns>Success, or an error if the operation failed.</returns>
    ErrorOr<Success> Create(User user);

    /// <summary>
    /// Updates an existing user record.
    /// </summary>
    /// <param name="user">The user entity with updated values.</param>
    /// <returns>Success, or an error if the operation failed.</returns>
    ErrorOr<Success> Update(User user);

    /// <summary>
    /// Updates the password hash for the specified user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="newPasswordHash">The new hashed password.</param>
    /// <returns>Success, or an error if the operation failed.</returns>
    ErrorOr<Success> UpdatePassword(int userId, string newPasswordHash);

    /// <summary>
    /// Increments the failed login attempt counter for the specified user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    void IncrementFailedAttempts(int userId);

    /// <summary>
    /// Resets the failed login attempt counter to zero for the specified user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    void ResetFailedAttempts(int userId);

    /// <summary>
    /// Locks the specified user account until the given time.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="lockoutEnd">The UTC time when the lockout expires.</param>
    void LockAccount(int userId, DateTime lockoutEnd);
}