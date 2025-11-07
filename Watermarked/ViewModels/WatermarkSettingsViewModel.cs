using CommunityToolkit.Mvvm.ComponentModel;

namespace Watermarked.ViewModels;

public partial class WatermarkSettingsViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _watermarkText = "Your Watermark";
}