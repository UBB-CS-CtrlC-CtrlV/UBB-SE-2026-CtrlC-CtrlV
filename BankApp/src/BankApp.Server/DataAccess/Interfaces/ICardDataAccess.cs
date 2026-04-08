using BankApp.Contracts.Entities;

namespace BankApp.Server.DataAccess.Interfaces;

/// <summary>
/// Defines data access operations for payment cards.
/// </summary>
public interface ICardDataAccess
{
    /// <summary>
    /// Finds all cards belonging to the specified user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <returns>A list of cards owned by the user.</returns>
    List<Card> FindByUserId(int userId);

    /// <summary>
    /// Finds a card by its unique identifier.
    /// </summary>
    /// <param name="id">The card identifier.</param>
    /// <returns>The matching <see cref="Card"/>, or <see langword="null"/> if not found.</returns>
    Card? FindById(int id);
}