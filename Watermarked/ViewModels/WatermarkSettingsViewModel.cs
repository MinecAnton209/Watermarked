using Avalonia.Media;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Watermarked.Models;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.Linq;
using System;
using System.Collections.Generic;

namespace Watermarked.ViewModels;

public partial class WatermarkSettingsViewModel : ViewModelBase
{
    [ObservableProperty]
    private WatermarkType _watermarkType = WatermarkType.Text;
    
    [ObservableProperty] private string _watermarkText = "Your Watermark";
    [ObservableProperty] private double _fontSize = 48;
    [ObservableProperty] private Color _textColor = Colors.White;
    [ObservableProperty] private bool _isTextWrappingEnabled = false;
    [ObservableProperty] private double _opacity = 0.6;
    [ObservableProperty] private FontFamily _selectedFontFamily;
    public IEnumerable<FontFamily> SystemFonts { get; }
    
    [ObservableProperty]
    private string? _imagePath;
    [ObservableProperty]
    private double _imageScale = 20;
    [ObservableProperty]
    private Bitmap? _watermarkPreviewImage;
    
    public PositioningViewModel Position { get; } = new();

    public WatermarkSettingsViewModel()
    {
        SystemFonts = FontManager.Current.SystemFonts;
        _selectedFontFamily = SystemFonts.FirstOrDefault() ?? FontFamily.Default;
    }
    
    partial void OnImagePathChanged(string? value)
    {
        WatermarkPreviewImage?.Dispose();
        WatermarkPreviewImage = null;
        if (value is not null)
        {
            try { WatermarkPreviewImage = new Bitmap(value); }
            catch (Exception e) { Console.WriteLine($"Failed to load watermark image: {e.Message}"); }
        }
    }

    [RelayCommand]
    private async Task SelectWatermarkImage(TopLevel? topLevel)
    {
        if (topLevel is null) return;
        var result = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Watermark Image",
            AllowMultiple = false,
            FileTypeFilter = new[] { FilePickerFileTypes.ImageAll }
        });
        if (result.Any())
        {
            ImagePath = result.First().Path.LocalPath;
        }
    }
}