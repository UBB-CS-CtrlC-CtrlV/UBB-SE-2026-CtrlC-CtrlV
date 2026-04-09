using BankApp.Contracts.Entities;
using ErrorOr;

namespace BankApp.Server.DataAccess.Interfaces;

/// <summary>
/// Defines data access operations for notification preferences.
/// </summary>
public interface INotificationPreferenceDataAccess
{
    /// <summary>
    /// Creates a default notification preference for the given user and category.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="category">The notification category name.</param>
    /// <returns><see langword="true"/> if the preference was created successfully; otherwise, <see langword="false"/>.</returns>
    ErrorOr<Success> Create(int userId, string category);

    /// <summary>
    /// Finds all notification preferences for the specified user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <returns>A list of notification preferences for the user.</returns>
    List<NotificationPreference> FindByUserId(int userId);

    /// <summary>
    /// Replaces all notification preferences for the specified user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="preferences">The updated list of notification preferences.</param>
    /// <returns><see langword="true"/> if the update succeeded; otherwise, <see langword="false"/>.</returns>
    ErrorOr<Success> Update(int userId, List<NotificationPreference> preferences);
}