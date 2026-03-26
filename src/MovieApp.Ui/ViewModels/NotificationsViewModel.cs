using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using MovieApp.Core.Models;
using MovieApp.Core.Services;

namespace MovieApp.Ui.ViewModels;

/// <summary>
/// Provides the notifications screen with the current user's persisted notifications.
/// </summary>
public sealed partial class NotificationsViewModel : ObservableObject
{
    private readonly INotificationService? _notificationService;
    private readonly int _currentUserId;

    [ObservableProperty]
    public partial Notification? SelectedNotification { get; set; }

    /// <summary>
    /// Gets the current page-level notification collection.
    /// </summary>
    public ObservableCollection<Notification> Notifications { get; } = new();

    /// <summary>
    /// Gets a value indicating whether the notification service is available.
    /// </summary>
    public bool IsServiceAvailable => _notificationService is not null && _currentUserId != 0;

    /// <summary>
    /// Gets the visibility of the notifications status message.
    /// </summary>
    public Visibility StatusVisibility => IsServiceAvailable ? Visibility.Collapsed : Visibility.Visible;

    /// <summary>
    /// Gets the status message shown when notifications cannot be loaded.
    /// </summary>
    public string StatusMessage => "Notifications are unavailable because the database connection is not ready.";

    /// <summary>
    /// Gets the command that opens the event related to the selected notification.
    /// </summary>
    public ICommand OpenEventCommand { get; }

    /// <summary>
    /// Gets the command that acknowledges and removes the selected notification.
    /// </summary>
    public ICommand MarkAsReadCommand { get; }

    /// <summary>
    /// Creates the view model from the application-level notification service.
    /// </summary>
    public NotificationsViewModel()
    {
        _notificationService = App.NotificationService;
        _currentUserId = App.CurrentUserId;

        OpenEventCommand = new RelayCommand(OpenEvent, () => SelectedNotification is not null);
        MarkAsReadCommand = new AsyncRelayCommand(MarkAsReadAsync, () => SelectedNotification is not null && IsServiceAvailable);
    }

    partial void OnSelectedNotificationChanged(Notification? value)
    {
        ((RelayCommand)OpenEventCommand).NotifyCanExecuteChanged();
        ((AsyncRelayCommand)MarkAsReadCommand).NotifyCanExecuteChanged();
    }

    /// <summary>
    /// Loads the current user's notifications from the database-backed service.
    /// </summary>
    public async Task InitializeAsync()
    {
        Notifications.Clear();

        if (!IsServiceAvailable)
        {
            return;
        }

        var notifications = await _notificationService!.GetNotificationsByUserIdAsync(_currentUserId);
        foreach (var notification in notifications)
        {
            Notifications.Add(notification);
        }
    }

    private void OpenEvent()
    {
        // Navigation logic for event details would go here.
    }

    /// <summary>
    /// Removes the selected notification after it has been acknowledged.
    /// </summary>
    private async Task MarkAsReadAsync()
    {
        if (SelectedNotification is null || !IsServiceAvailable)
        {
            return;
        }

        await _notificationService!.MarkAsReadOrRemoveAsync(SelectedNotification.Id);
        Notifications.Remove(SelectedNotification);
        SelectedNotification = null;
    }
}
