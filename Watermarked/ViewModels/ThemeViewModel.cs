using System;
using Avalonia;
using Avalonia.Platform;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using Watermarked.Models;

namespace Watermarked.ViewModels;

public partial class ThemeViewModel : ViewModelBase
{
    [ObservableProperty]
    private ThemeType _selectedTheme = ThemeType.System; 

    public ThemeViewModel()
    {
        var a = Application.Current;
        if (a?.PlatformSettings is not null)
        {
            a.PlatformSettings.ColorValuesChanged += OnPlatformColorValuesChanged;
        }
        
        ApplyTheme();
    }

    private void OnPlatformColorValuesChanged(object? sender, PlatformColorValues e)
    {
        if (SelectedTheme == ThemeType.System)
        {
            ApplyTheme();
        }
    }

    partial void OnSelectedThemeChanged(ThemeType value)
    {
        ApplyTheme();
    }

    private void ApplyTheme()
    {
        if (Application.Current is null) return;
        
        var theme = SelectedTheme;
        if (theme == ThemeType.System)
        {
            theme = Application.Current.PlatformSettings?.GetColorValues().ThemeVariant == PlatformThemeVariant.Light
                ? ThemeType.Light
                : ThemeType.Dark;
        }

        Application.Current.RequestedThemeVariant = theme == ThemeType.Light 
            ? ThemeVariant.Light 
            : ThemeVariant.Dark;
    }
}