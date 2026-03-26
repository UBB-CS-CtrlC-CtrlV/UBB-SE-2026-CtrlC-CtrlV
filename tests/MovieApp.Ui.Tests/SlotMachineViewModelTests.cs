using MovieApp.Ui.ViewModels;
using Xunit;

namespace MovieApp.Ui.Tests;

/// <summary>
/// Covers non-interactive shell states for the slot-machine page.
/// </summary>
public sealed class SlotMachineViewModelTests
{
    [Fact]
    public void CreateUnavailable_DisablesSpinAndPreservesOfflineMessage()
    {
        var viewModel = SlotMachineViewModel.CreateUnavailable(
            "Slot machine unavailable because the database connection is not ready.");

        Assert.Equal(0, viewModel.AvailableSpins);
        Assert.False(viewModel.IsSpinButtonEnabled);
        Assert.False(viewModel.SpinCommand.CanExecute(null));
        Assert.Equal(
            "Slot machine unavailable because the database connection is not ready.",
            viewModel.StatusMessage);
    }
}
