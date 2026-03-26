using System.Collections.ObjectModel;
using MovieApp.Core.Models;
using MovieApp.Core.Services;

namespace MovieApp.Ui.ViewModels.Events;

public sealed class NotificationsViewModel : ViewModelBase
{
    private readonly INotificationService _notificationService;
    private bool _isLoading;

    public NotificationsViewModel()
    {
        _notificationService = App.NotificationService ?? throw new InvalidOperationException("NotificationService is not initialized.");
        Notifications = new ObservableCollection<Notification>();
    }

    public ObservableCollection<Notification> Notifications { get; }

    public bool IsLoading
    {
        get => _isLoading;
        private set => SetProperty(ref _isLoading, value);
    }
    
    public bool HasNoNotifications => !IsLoading && Notifications.Count == 0;

    public async Task InitializeAsync()
    {
        IsLoading = true;
        OnPropertyChanged(nameof(HasNoNotifications));
        
        try
        {
            var currentUser = App.CurrentUserService?.CurrentUser;
            if (currentUser == null) return;

            var notifications = await _notificationService.GetNotificationsByUserAsync(currentUser.Id);
            
            Notifications.Clear();
            foreach (var n in notifications)
            {
                Notifications.Add(n);
            }
            OnPropertyChanged(nameof(HasNoNotifications));
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task RemoveNotificationAsync(int notificationId)
    {
        await _notificationService.RemoveNotificationAsync(notificationId);
        await InitializeAsync();
    }
}
