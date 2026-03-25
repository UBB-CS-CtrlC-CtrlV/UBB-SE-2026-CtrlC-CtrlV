using Microsoft.UI.Xaml.Controls;
using MovieApp.Ui.ViewModels.Events;

namespace MovieApp.Ui.Controls;

public sealed partial class SeatGuideDialog : ContentDialog
{
    public SeatGuideViewModel ViewModel { get; }

    public SeatGuideDialog(int totalCapacity)
    {
        this.InitializeComponent();
        
        ViewModel = new SeatGuideViewModel(totalCapacity);
        this.DataContext = ViewModel;
    }
}