using BankApp.Domain.Entities;
using ErrorOr;

namespace BankApp.Infrastructure.DataAccess.Interfaces;

/// <summary>
/// Defines data access operations for user notifications.
/// </summary>
public interface INotificationDataAccess
{
    /// <summary>Finds all notifications belonging to the specified user.</summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <returns>A list of notifications for the user, or an error if the operation failed.</returns>
    ErrorOr<List<Notification>> FindByUserId(int userId);

    /// <summary>Counts the number of unread notifications for the specified user.</summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <returns>The count of unread notifications, or an error if the operation failed.</returns>
    ErrorOr<int> CountUnreadByUserId(int userId);
}
