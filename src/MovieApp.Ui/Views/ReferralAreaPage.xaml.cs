using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;

namespace MovieApp.Ui.Views;

/// <summary>
/// Exposes the referral-code feature area, including ambassador progress,
/// usage tracking, and reward-threshold handoff points.
/// </summary>
public sealed partial class ReferralAreaPage : Page, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private string _referralCode = string.Empty;
    public string ReferralCode
    {
        get => _referralCode;
        set
        {
            if (_referralCode != value)
            {
                _referralCode = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ReferralCode)));
            }
        }
    }

    public ReferralAreaPage()
    {
        InitializeComponent();
        Loaded += ReferralAreaPage_Loaded;
    }

    private async void ReferralAreaPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (App.AmbassadorRepository is not null && App.CurrentUserService?.CurrentUser is { } currentUser)
        {
            var code = await App.AmbassadorRepository.GetReferralCodeAsync(currentUser.Id);
            ReferralCode = code ?? "No code generated";
        }
    }
}
