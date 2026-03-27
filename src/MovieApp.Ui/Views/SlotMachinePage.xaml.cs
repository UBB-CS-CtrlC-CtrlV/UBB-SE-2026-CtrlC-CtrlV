using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using MovieApp.Core.Models;
using MovieApp.Core.Models.Movie;
using MovieApp.Ui.Controls;
using MovieApp.Ui.ViewModels;
using System.Globalization;
using Windows.UI;

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
        this.Focus(FocusState.Programmatic);
    }

    private async void OnJackpotHit(Movie movie, int discountPercentage)
    {
        var gold   = new SolidColorBrush(Color.FromArgb(0xFF, 0xC8, 0x97, 0x1A));
        var darkBg = new SolidColorBrush(Color.FromArgb(0xFF, 0x0B, 0x0B, 0x1E));
        var lightText = new SolidColorBrush(Color.FromArgb(0xFF, 0xE8, 0xE8, 0xFF));

        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            DefaultButton = ContentDialogButton.None,
        };

        // Casino colour overrides (title area, border, background)
        dialog.Resources["ContentDialogBackground"]      = darkBg;
        dialog.Resources["ContentDialogForeground"]      = lightText;
        dialog.Resources["ContentDialogTitleForeground"] = gold;
        dialog.Resources["ContentDialogBorderBrush"]     = gold;

        // Content is built after the dialog so the close button can reference it
        dialog.Content = BuildJackpotDialogContent(movie, discountPercentage, dialog);

        await dialog.ShowAsync();
        this.Focus(FocusState.Programmatic);
    }

    private static UIElement BuildJackpotDialogContent(Movie movie, int discountPercentage, ContentDialog dialog)
    {
        var gold      = new SolidColorBrush(Color.FromArgb(0xFF, 0xC8, 0x97, 0x1A));
        var goldHover = new SolidColorBrush(Color.FromArgb(0xFF, 0xE0, 0xAA, 0x20));
        var goldPress = new SolidColorBrush(Color.FromArgb(0xFF, 0xA0, 0x78, 0x14));
        var dimGold   = new SolidColorBrush(Color.FromArgb(0xFF, 0x6A, 0x4E, 0x0F));
        var dimText   = new SolidColorBrush(Color.FromArgb(0xFF, 0xA0, 0xA0, 0xC0));
        var goldBg    = new SolidColorBrush(Color.FromArgb(0x40, 0xC8, 0x97, 0x1A));
        var darkText  = new SolidColorBrush(Color.FromArgb(0xFF, 0x0B, 0x0B, 0x1E));
        var ruleBrush = new SolidColorBrush(Color.FromArgb(0x80, 0xC8, 0x97, 0x1A));

        var layout = new StackPanel
        {
            Spacing = 18,
            HorizontalAlignment = HorizontalAlignment.Center,
            MinWidth = 320,
        };

        // ── Title (centered, owned by content so we control alignment) ──
        layout.Children.Add(new TextBlock
        {
            Text = "🎰  JACKPOT!  🎰",
            FontSize = 22,
            FontWeight = FontWeights.Bold,
            Foreground = gold,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            CharacterSpacing = 40,
        });

        layout.Children.Add(new Rectangle { Height = 1, Fill = ruleBrush, HorizontalAlignment = HorizontalAlignment.Stretch });
        layout.Children.Add(BuildLightStrip(gold, dimGold));
        layout.Children.Add(new Rectangle { Height = 1, Fill = ruleBrush, HorizontalAlignment = HorizontalAlignment.Stretch });

        layout.Children.Add(new TextBlock
        {
            Text = movie.Title,
            FontSize = 22,
            FontWeight = FontWeights.Bold,
            Foreground = gold,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 340,
        });

        layout.Children.Add(new TextBlock
        {
            Text = "All three reels aligned — you hit the jackpot!",
            FontSize = 12,
            Foreground = dimText,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center,
        });

        layout.Children.Add(new Border
        {
            Background = goldBg,
            BorderBrush = gold,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(24, 14, 24, 14),
            HorizontalAlignment = HorizontalAlignment.Center,
            Child = new StackPanel
            {
                Spacing = 2,
                HorizontalAlignment = HorizontalAlignment.Center,
                Children =
                {
                    new TextBlock
                    {
                        Text = $"{discountPercentage}%",
                        FontSize = 44,
                        FontWeight = FontWeights.Bold,
                        Foreground = gold,
                        HorizontalAlignment = HorizontalAlignment.Center,
                    },
                    new TextBlock
                    {
                        Text = "DISCOUNT EARNED",
                        FontSize = 11,
                        FontWeight = FontWeights.Bold,
                        Foreground = gold,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        CharacterSpacing = 120,
                    },
                },
            },
        });

        layout.Children.Add(new Rectangle { Height = 1, Fill = ruleBrush, HorizontalAlignment = HorizontalAlignment.Stretch });
        layout.Children.Add(BuildLightStrip(dimGold, gold));
        layout.Children.Add(new Rectangle { Height = 1, Fill = ruleBrush, HorizontalAlignment = HorizontalAlignment.Stretch });

        // ── Collect button (centered, gold, owned by content) ──
        var collectButton = new Button
        {
            Content = "🎉  Collect!",
            HorizontalAlignment = HorizontalAlignment.Center,
            Background = gold,
            Foreground = darkText,
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(32, 12, 32, 12),
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            IsTabStop = false,
        };
        collectButton.Resources["ButtonBackgroundPointerOver"] = goldHover;
        collectButton.Resources["ButtonForegroundPointerOver"] = darkText;
        collectButton.Resources["ButtonBackgroundPressed"]     = goldPress;
        collectButton.Resources["ButtonForegroundPressed"]     = darkText;
        collectButton.Click += (_, _) => dialog.Hide();
        layout.Children.Add(collectButton);

        return layout;
    }

    private static StackPanel BuildLightStrip(SolidColorBrush primary, SolidColorBrush secondary)
    {
        var strip = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            Spacing = 8,
        };
        for (int i = 0; i < 11; i++)
            strip.Children.Add(new Ellipse { Width = 10, Height = 10, Fill = i % 2 == 0 ? primary : secondary });
        return strip;
    }

    private async void ViewEventButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.DataContext is not MatchingEventItem item)
            return;

        var selectedEvent = item.Event;
        var content = BuildEventDialogContent(selectedEvent, item.IsJackpotEvent);

        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Title = selectedEvent.Title,
            PrimaryButtonText = "Close",
            DefaultButton = ContentDialogButton.Primary,
            Content = content,
        };

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
                    dialog.Hide();

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

        await dialog.ShowAsync();
        this.Focus(FocusState.Programmatic);

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

