using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Watermarked.ViewModels;

public partial class WatermarkSettingsViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _watermarkText = "Your Watermark";

    [ObservableProperty]
    private double _fontSize = 48;

    [ObservableProperty]
    private Color _textColor = Colors.White;
    
    [ObservableProperty]
    private bool _isTextWrappingEnabled = false;
}