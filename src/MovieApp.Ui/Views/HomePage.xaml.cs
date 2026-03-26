using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;
using MovieApp.Ui.Controls;
using MovieApp.Ui.Navigation;
using MovieApp.Ui.Services;
using MovieApp.Ui.ViewModels.Events;
using System;

namespace MovieApp.Ui.Views;

/// <summary>
/// Hosts the discovery-first home experience, including horizontal event sections
/// and launch points into the other requirement-driven feature areas.
/// </summary>
public sealed partial class HomePage : Page
{
    private bool _initialized;

    public HomeEventsViewModel ViewModel { get; }

    /// <summary>
    /// Returns the repository used to populate the home event rows.
    /// </summary>
    private static IEventRepository GetEventRepository()
        => App.EventRepository ?? UnavailableEventRepository.Instance;

    public HomePage()
    {
        ViewModel = new HomeEventsViewModel(GetEventRepository());
        NavigationCacheMode = NavigationCacheMode.Required;
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += HomePage_Loaded;
    }

    /// <summary>
    /// Initializes the page's event data once after the visual tree is loaded.
    /// </summary>
    private async void HomePage_Loaded(object sender, RoutedEventArgs e)
    {
        if (_initialized) return;
        _initialized = true;

        if (App.AmbassadorRepository is not null && App.CurrentUserService?.CurrentUser is { } currentUser)
        {
            var existingCode = await App.AmbassadorRepository.GetReferralCodeAsync(currentUser.Id);
            if (string.IsNullOrEmpty(existingCode))
            {
                var generator = new MovieApp.Core.Services.ReferralCodeGenerator();
                var newCode = generator.Generate(currentUser.Username, currentUser.Id);
                await App.AmbassadorRepository.CreateAmbassadorProfileAsync(currentUser.Id, newCode);
            }
        }

        await ViewModel.InitializeAsync();
    }

    private void ShortcutButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.DataContext is not HomeNavigationShortcut shortcut)
        {
            return;
        }

        if (App.CurrentMainWindow is not null)
        {
            App.CurrentMainWindow.NavigateToRoute(shortcut.RouteTag);
            return;
        }

        Frame.Navigate(AppRouteResolver.ResolvePageType(shortcut.RouteTag));
    }

    /// <summary>
    /// Applies the current search text to the home page event list only.
    /// </summary>
    private void SearchBox_SearchTextChanged(object? sender, string searchText)
    {
        ViewModel.SetSearchText(searchText);
    }

    /// <summary>
    /// Applies the selected sort mode to the home page event list only.
    /// </summary>
    private void SortSelector_SortOptionChanged(object? sender, MovieApp.Core.EventLists.EventSortOption sortOption)
    {
        ViewModel.SetSortOption(sortOption);
    }

    private async void EventCardButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.DataContext is not Event selectedEvent)
        {
            return;
        }

        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Title = selectedEvent.Title,
            PrimaryButtonText = "Close",
            DefaultButton = ContentDialogButton.Primary
        };

        var content = BuildEventDialogContent(selectedEvent, dialog);

        // Check reward balance and add a Free Pass button if available
        if (App.AmbassadorRepository is not null && App.CurrentUserService?.CurrentUser is { } currentUser)
        {
            int balance = await App.AmbassadorRepository.GetRewardBalanceAsync(currentUser.Id);
            if (balance > 0 && content is StackPanel layout)
            {
                var freePassButton = new Button
                {
                    Content = $"Use Free Pass 🎟 ({balance} left)",
                    HorizontalAlignment = HorizontalAlignment.Left,
                };

                freePassButton.Click += async (_, _) =>
                {
                    var confirmDialog = new ContentDialog
                    {
                        XamlRoot = XamlRoot,
                        Title = "Use Free Pass?",
                        Content = "This will use 1 free enrollment credit. Continue?",
                        PrimaryButtonText = "Yes, enroll for free",
                        CloseButtonText = "Cancel",
                        DefaultButton = ContentDialogButton.Primary,
                    };

                    var result = await confirmDialog.ShowAsync();
                    if (result == ContentDialogResult.Primary)
                    {
                        await App.AmbassadorRepository.DecrementRewardBalanceAsync(currentUser.Id);
                        freePassButton.Content = "✅ Free Pass applied!";
                        freePassButton.IsEnabled = false;
                    }
                };

                layout.Children.Add(freePassButton);
            }
        }

        dialog.Content = content;

        await dialog.ShowAsync();
    }

    private static UIElement BuildEventDialogContent(Event selectedEvent, ContentDialog parentDialog)
    {
        var layout = new StackPanel
        {
            Spacing = 12,
        };

        layout.Children.Add(new TextBlock
        {
            Text = selectedEvent.Description,
            TextWrapping = TextWrapping.WrapWholeWords,
        });

        layout.Children.Add(new TextBlock
        {
            Text = $"When: {selectedEvent.EventDateTime:g}",
        });

        layout.Children.Add(new TextBlock
        {
            Text = $"Where: {selectedEvent.LocationReference}",
        });

        layout.Children.Add(new TextBlock
        {
            Text = $"Price: {EventCard.GetPriceText(selectedEvent, System.Globalization.CultureInfo.CurrentCulture)}",
        });

        layout.Children.Add(new TextBlock
        {
            Text = $"Rating: {EventCard.GetRatingText(selectedEvent)}",
        });

        layout.Children.Add(new TextBlock
        {
            Text = $"Seats: {EventCard.GetCapacityText(selectedEvent)}",
        });

        var referralTextBox = new TextBox { PlaceholderText = "Optional referral code", Width = 200 };
        var validationButton = new Button { Content = new FontIcon { Glyph = "\uE73E", FontSize = 14 } };

        validationButton.Click += async (s, e) =>
        {
            if (string.IsNullOrWhiteSpace(referralTextBox.Text)) return;

            if (App.ReferralValidator is not null && App.CurrentUserService?.CurrentUser is { } currentUser)
            {
                bool isValid = await App.ReferralValidator.IsValidReferralAsync(referralTextBox.Text, currentUser.Id);
                referralTextBox.BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    isValid ? Microsoft.UI.Colors.Green : Microsoft.UI.Colors.Red);
                referralTextBox.BorderThickness = new Microsoft.UI.Xaml.Thickness(2);
            }
        };

        layout.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children = { referralTextBox, validationButton }
        });

        var seatGuideButton = new Button { Content = "Seat guide" };
        seatGuideButton.Click += async (s, args) =>
        {
            parentDialog.Hide();
            
            try
            {
                int capacity = selectedEvent.MaxCapacity > 0 ? selectedEvent.MaxCapacity : 50;
                var seatDialog = new SeatGuideDialog(capacity)
                {
                    XamlRoot = parentDialog.XamlRoot
                };
                await seatDialog.ShowAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Eroare la deschiderea Seat Guide: {ex.Message}");
            }
        };

        layout.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                new Button { Content = "Will attend" },
                new Button { Content = "Buy ticket" },
                new Button { Content = "Favorite" },
                seatGuideButton
            },
        });

        return layout;
    }
}
