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

/// <summary>
/// Hosts the application shell, navigation structure, and the top-level frame
/// where each requirement-driven feature page is loaded.
/// </summary>
public sealed partial class MainWindow : Window
{
    private readonly IEventRepository _eventRepository;

    public MainWindow(MainViewModel viewModel, IEventRepository eventRepository)
    {
        ViewModel = viewModel;
        _eventRepository = eventRepository; // Injectăm Repository-ul pentru evenimente reale
        InitializeComponent();

        // Ne legăm de evenimentul de activare a ferestrei pentru verificarea inițială
        this.Activated += MainWindow_Activated;

        NavigateToRoute(AppRouteResolver.Home);
    }

    public MainViewModel ViewModel { get; }

    private async void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        // Ne dezabonăm pentru a ne asigura că se execută o singură dată, la prima deschidere
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

            // Verificăm fiecare eveniment în parte
            foreach (var watched in watchedEvents)
            {
                var realEvent = await _eventRepository.FindByIdAsync(watched.EventId);
                
                // Dacă prețul real este mai mic sau egal cu cel urmărit de utilizator
                if (realEvent != null && realEvent.TicketPrice <= watched.TargetPrice)
                {
                    priceDroppedMessages.Add($"Target reached! '{realEvent.Title}' is now {realEvent.TicketPrice:C}");
                    
                    // Ștergem evenimentul din watchlist pentru a nu-l mai notifica la infinit
                    await watcherRepo.RemoveWatchAsync(watched.EventId);
                }
            }

            // Afișăm InfoBar-ul dacă există alerte
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

    private void AlertsButton_Click(object sender, RoutedEventArgs e)
    {
        NavigateToRoute(AppRouteResolver.Notifications);
    }

    private void RewardsButton_Click(object sender, RoutedEventArgs e)
    {
        NavigateToRoute(AppRouteResolver.Rewards);
    }

    private void ReferralButton_Click(object sender, RoutedEventArgs e)
    {
        NavigateToRoute(AppRouteResolver.ReferralArea);
    }

    private void SyncSelectedNavigationItem(string tag)
    {
        var selectedItem = AppNavigationView.MenuItems
            .OfType<NavigationViewItem>()
            .FirstOrDefault(item => string.Equals(item.Tag as string, tag, StringComparison.Ordinal));

        AppNavigationView.SelectedItem = selectedItem;
    }
}