using BankApp.Contracts.Entities;

namespace BankApp.Server.DataAccess.Interfaces;

/// <summary>
/// Defines data access operations for user notifications.
/// </summary>
public interface INotificationDataAccess
{
    /// <summary>
    /// Finds all notifications belonging to the specified user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <returns>A list of notifications for the user.</returns>
    List<Notification> FindByUserId(int userId);

    /// <summary>
    /// Counts the number of unread notifications for the specified user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <returns>The count of unread notifications.</returns>
    int CountUnreadByUserId(int userId);
}