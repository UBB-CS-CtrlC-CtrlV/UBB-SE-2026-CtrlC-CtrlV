using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MovieApp.Core.Models;
using MovieApp.Core.Models.Movie;
using MovieApp.Ui.Controls;
using MovieApp.Ui.ViewModels;
using System.Globalization;
using Windows.System;

namespace MovieApp.Ui.Views;

/// <summary>
/// Hosts the slot-machine game surface, its spin economy, matching results,
/// and jackpot-reward plug-in regions.
/// </summary>
public sealed partial class SlotMachinePage : Page
{
    /// <summary>
    /// Creates the slot-machine page and defers database-backed initialization
    /// until the page is loaded into the shell.
    /// </summary>
    public SlotMachinePage()
    {
        InitializeComponent();
        Loaded += OnPageLoaded;
        AddHandler(KeyDownEvent, new KeyEventHandler(OnKeyDown), handledEventsToo: true);
    }

    private async void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnPageLoaded;

        var currentUser = App.CurrentUserService?.CurrentUser;
        if (currentUser is null)
            return;

        var viewModel = new SlotMachineViewModel(
            currentUser.Id,
            App.SlotMachineService ?? throw new InvalidOperationException("SlotMachineService not initialized"),
            App.SlotMachineAnimationService ?? throw new InvalidOperationException("SlotMachineAnimationService not initialized"));

        viewModel.JackpotHit += OnJackpotHit;
        DataContext = viewModel;
        await viewModel.InitializeAsync();
    }

    private void OnKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Space)
        {
            if (DataContext is SlotMachineViewModel vm && vm.SpinCommand.CanExecute(null))
            {
                vm.SpinCommand.Execute(null);
            }

            e.Handled = true;
        }
    }

    private async void OnJackpotHit(Movie movie, int discountPercentage)
    {
        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Title = "🎰 JACKPOT! 🎰",
            PrimaryButtonText = "Awesome!",
            DefaultButton = ContentDialogButton.Primary,
            Content = BuildJackpotDialogContent(movie, discountPercentage),
        };

        await dialog.ShowAsync();
    }

    private static UIElement BuildJackpotDialogContent(Movie movie, int discountPercentage)
    {
        var layout = new StackPanel { Spacing = 16, HorizontalAlignment = HorizontalAlignment.Center };

        layout.Children.Add(new TextBlock
        {
            Text = "🎰  🎰  🎰",
            FontSize = 48,
            HorizontalAlignment = HorizontalAlignment.Center,
        });

        layout.Children.Add(new TextBlock
        {
            Text = "All three reels matched a movie!",
            FontSize = 14,
            HorizontalAlignment = HorizontalAlignment.Center,
            Opacity = 0.8,
        });

        layout.Children.Add(new TextBlock
        {
            Text = movie.Title,
            FontSize = 22,
            FontWeight = Microsoft.UI.Text.FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
        });

        var discountBorder = new Border
        {
            Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                Windows.UI.Color.FromArgb(0x40, 0x16, 0xA3, 0x4A)),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(16, 12, 16, 12),
            HorizontalAlignment = HorizontalAlignment.Center,
            Child = new TextBlock
            {
                Text = $"🎉 You earned a {discountPercentage}% discount!",
                FontSize = 18,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Center,
            },
        };
        layout.Children.Add(discountBorder);

        return layout;
    }

    private async void ViewEventButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.DataContext is not MatchingEventItem item)
            return;

        var selectedEvent = item.Event;
        var content = BuildEventDialogContent(selectedEvent, item.IsJackpotEvent);

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
                        await App.AmbassadorRepository!.DecrementRewardBalanceAsync(currentUser.Id);
                        freePassButton.Content = "✅ Free Pass applied!";
                        freePassButton.IsEnabled = false;
                    }
                };

                layout.Children.Add(freePassButton);
            }
        }

        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Title = selectedEvent.Title,
            PrimaryButtonText = "Close",
            DefaultButton = ContentDialogButton.Primary,
            Content = content,
        };

        await dialog.ShowAsync();

        // SM.30: immediately update the spin counter after a possible bonus-spin grant
        if (DataContext is SlotMachineViewModel vm)
            await vm.RefreshSpinCountAsync();
    }

    private static UIElement BuildEventDialogContent(Event selectedEvent, bool isJackpotEvent)
    {
        var layout = new StackPanel { Spacing = 12 };

        layout.Children.Add(new TextBlock
        {
            Text = selectedEvent.Description ?? "A curated movie experience with limited seating.",
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
            Text = $"Price: {EventCard.GetPriceText(selectedEvent, CultureInfo.CurrentCulture)}",
        });

        layout.Children.Add(new TextBlock
        {
            Text = $"Rating: {EventCard.GetRatingText(selectedEvent)}",
        });

        layout.Children.Add(new TextBlock
        {
            Text = $"Seats: {EventCard.GetCapacityText(selectedEvent)}",
        });

        if (isJackpotEvent)
        {
            var discountPct = EventCard.DiscountByEventId.TryGetValue(selectedEvent.Id, out var pct) ? pct : 70;
            var discountBanner = new Border
            {
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(0x33, 0xFF, 0xC1, 0x07)),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(12, 8, 12, 8),
                Margin = new Thickness(0, 4, 0, 4),
                Child = new StackPanel
                {
                    Spacing = 4,
                    Children =
                    {
                        new TextBlock
                        {
                            Text = $"⭐ {discountPct}% Jackpot Discount applies to this event!",
                            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                        },
                        new TextBlock
                        {
                            Text = $"Discounted price: {EventCard.GetDiscountedPriceText(selectedEvent, CultureInfo.CurrentCulture, discountPct)}",
                            FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                            FontSize = 16,
                        },
                    },
                },
            };
            layout.Children.Add(discountBanner);
        }

        var referralTextBox = new TextBox { PlaceholderText = "Optional referral code", Width = 200 };
        var validationButton = new Button { Content = new FontIcon { Glyph = "\uE73E", FontSize = 14 } };

        validationButton.Click += async (s, args) =>
        {
            if (string.IsNullOrWhiteSpace(referralTextBox.Text)) return;

            if (App.ReferralValidator is not null && App.CurrentUserService?.CurrentUser is { } currentUser)
            {
                bool isValid = await App.ReferralValidator.IsValidReferralAsync(referralTextBox.Text, currentUser.Id);
                referralTextBox.BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    isValid ? Microsoft.UI.Colors.Green : Microsoft.UI.Colors.Red);
                referralTextBox.BorderThickness = new Thickness(2);
            }
        };

        layout.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children = { referralTextBox, validationButton },
        });

        var willAttendBtn = new Button { Content = "Will attend", Tag = "Joined!" };
        var buyTicketBtn = new Button { Content = "Buy ticket", Tag = "Ticket purchased!" };
        EventCard.AttachJoinEventHandler(willAttendBtn, selectedEvent.Id);
        EventCard.AttachJoinEventHandler(buyTicketBtn, selectedEvent.Id);

        layout.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                willAttendBtn,
                buyTicketBtn,
                new Button { Content = "Favorite" },
                new Button { Content = "Seat guide" },
            },
        });

        return layout;
    }
}

