using BankApp.Domain.Entities;
using ErrorOr;

namespace BankApp.Infrastructure.DataAccess.Interfaces;

/// <summary>
/// Defines data access operations for notification preferences.
/// </summary>
public interface INotificationPreferenceDataAccess
{
    /// <summary>Creates a default notification preference for the given user and category.</summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="category">The notification category name.</param>
    /// <returns>Success, or an error if the operation failed.</returns>
    ErrorOr<Success> Create(int userId, string category);

    /// <summary>Finds all notification preferences for the specified user.</summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <returns>A list of notification preferences for the user, or an error if the operation failed.</returns>
    ErrorOr<List<NotificationPreference>> FindByUserId(int userId);

    /// <summary>Updates all notification preferences for the specified user.</summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="preferences">The updated list of notification preferences.</param>
    /// <returns>Success, or an error if the operation failed.</returns>
    ErrorOr<Success> Update(int userId, List<NotificationPreference> preferences);
}
