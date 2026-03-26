using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MovieApp.Ui.ViewModels;

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
        if (currentUser is null
            || App.SlotMachineService is null
            || App.SlotMachineResultService is null
            || App.ReelAnimationService is null)
        {
            DataContext = SlotMachineViewModel.CreateUnavailable(
                "Slot machine unavailable because the database connection is not ready.");
            return;
        }

        var viewModel = new SlotMachineViewModel(
            currentUser.Id,
            App.SlotMachineService,
            App.SlotMachineResultService,
            App.ReelAnimationService);

        DataContext = viewModel;
        await viewModel.InitializeAsync();
    }
}

