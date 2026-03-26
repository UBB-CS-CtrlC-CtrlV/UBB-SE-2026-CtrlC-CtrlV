using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MovieApp.Core.Repositories;
using MovieApp.Ui.Navigation;
using MovieApp.Ui.ViewModels;

namespace MovieApp.Ui.Views;

public sealed partial class MainWindow : Window
{
    private readonly IEventRepository _eventRepository;

    public MainWindow(MainViewModel viewModel, IEventRepository eventRepository)
    {
        ViewModel = viewModel;
        _eventRepository = eventRepository;
        InitializeComponent();

        this.Activated += MainWindow_Activated;

        NavigateToRoute(AppRouteResolver.Home);
    }

    public MainViewModel ViewModel { get; }

    private async void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        this.Activated -= MainWindow_Activated;
        await CheckForPriceDropsAsync();
    }

    private async Task CheckForPriceDropsAsync()
    {
        try
        {
            var folderPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "MovieApp");
            if (!System.IO.Directory.Exists(folderPath)) return;

            var watcherRepo = new MovieApp.Infrastructure.LocalPriceWatcherRepository(folderPath);
            var watchedEvents = await watcherRepo.GetAllWatchedEventsAsync();
            
            if (!watchedEvents.Any()) return;

            var priceDroppedMessages = new List<string>();

            foreach (var watched in watchedEvents)
            {
                var realEvent = await _eventRepository.FindByIdAsync(watched.EventId);
                
                if (realEvent != null && realEvent.TicketPrice <= watched.TargetPrice)
                {
                    priceDroppedMessages.Add($"Target reached! '{realEvent.Title}' is now {realEvent.TicketPrice:C} (Target: {watched.TargetPrice:C})");
                    await watcherRepo.RemoveWatchAsync(watched.EventId);
                }
            }

            if (priceDroppedMessages.Any())
            {
                PriceAlertInfoBar.Message = string.Join("\n", priceDroppedMessages);
                PriceAlertInfoBar.IsOpen = true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error on startup check: {ex.Message}");
        }
    }

    public void NavigateToRoute(string tag)
    {
        var pageType = AppRouteResolver.ResolvePageType(tag);
        SyncSelectedNavigationItem(tag);

        if (ContentFrame.CurrentSourcePageType != pageType)
        {
            ContentFrame.Navigate(pageType);
        }
    }

    private void AppNavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItemContainer?.Tag is string tag)
        {
            NavigateToRoute(tag);
        }
    }

    private void SyncSelectedNavigationItem(string tag)
    {
        var selectedItem = AppNavigationView.MenuItems
            .OfType<NavigationViewItem>()
            .Concat(AppNavigationView.FooterMenuItems.OfType<NavigationViewItem>())
            .FirstOrDefault(item => string.Equals(item.Tag as string, tag, StringComparison.Ordinal));

        AppNavigationView.SelectedItem = selectedItem;
    }
}