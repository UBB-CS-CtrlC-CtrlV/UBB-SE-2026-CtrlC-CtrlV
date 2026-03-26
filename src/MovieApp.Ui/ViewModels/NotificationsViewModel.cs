using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MovieApp.Core.Models;
using MovieApp.Core.Services;

namespace MovieApp.Ui.ViewModels;

public sealed partial class NotificationsViewModel : ObservableObject
{
    private readonly INotificationService _notificationService;
    private readonly int _currentUserId;

    [ObservableProperty]
    public partial Notification? SelectedNotification { get; set; }

    public ObservableCollection<Notification> Notifications { get; } = new();

    public ICommand OpenEventCommand { get; }
    public ICommand MarkAsReadCommand { get; }

    public NotificationsViewModel()
    {
        _notificationService = App.NotificationService ?? throw new InvalidOperationException("NotificationService is not initialized.");
        _currentUserId = 1;

        OpenEventCommand = new RelayCommand(OpenEvent, () => SelectedNotification is not null);
        MarkAsReadCommand = new AsyncRelayCommand(MarkAsReadAsync, () => SelectedNotification is not null);
    }

    partial void OnSelectedNotificationChanged(Notification? value)
    {
        ((RelayCommand)OpenEventCommand).NotifyCanExecuteChanged();
        ((AsyncRelayCommand)MarkAsReadCommand).NotifyCanExecuteChanged();
    }

    public async Task InitializeAsync()
    {
        if (_currentUserId == 0) return;

        var notifications = await _notificationService.GetNotificationsByUserIdAsync(_currentUserId);
        Notifications.Clear();
        foreach (var notification in notifications)
        {
            Notifications.Add(notification);
        }
    }

    private void OpenEvent()
    {
        // Navigation logic for event details
    }

    private async Task MarkAsReadAsync()
    {
        if (SelectedNotification is null) return;

        await _notificationService.MarkAsReadOrRemoveAsync(SelectedNotification.Id);
        Notifications.Remove(SelectedNotification);
        SelectedNotification = null;
    }
}
