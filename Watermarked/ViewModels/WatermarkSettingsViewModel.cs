using System.Collections.Generic;
using System.Linq;
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
    private bool _isTextWrappingEnabled = true;
    
    [ObservableProperty]
    private FontFamily _selectedFontFamily;

    [ObservableProperty]
    private double _opacity = 0.6;
    
    public IEnumerable<FontFamily> SystemFonts { get; }
    public WatermarkSettingsViewModel()
    {
        SystemFonts = FontManager.Current.SystemFonts;
        
        _selectedFontFamily = SystemFonts.FirstOrDefault() ?? FontFamily.Default;
    }
}