using Avalonia;
using Avalonia.Layout;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Watermarked.ViewModels;

public partial class PositioningViewModel : ViewModelBase
{
    [ObservableProperty]
    private HorizontalAlignment _horizontalAlignment = HorizontalAlignment.Center;

    [ObservableProperty]
    private VerticalAlignment _verticalAlignment = VerticalAlignment.Center;

    [ObservableProperty]
    private Thickness _margin = new Thickness(0);

    [ObservableProperty]
    private Size _containerSize;

    [ObservableProperty]
    private Size _watermarkSize;

    [RelayCommand]
    private void SetAlignment(string parameter)
    {
        Margin = new Thickness(0);
    
        switch (parameter)
        {
            case "TopLeft":
                HorizontalAlignment = HorizontalAlignment.Left;
                VerticalAlignment = VerticalAlignment.Top;
                break;
            case "TopCenter":
                HorizontalAlignment = HorizontalAlignment.Center;
                VerticalAlignment = VerticalAlignment.Top;
                break;
            case "TopRight":
                HorizontalAlignment = HorizontalAlignment.Right;
                VerticalAlignment = VerticalAlignment.Top;
                break;
            case "CenterLeft":
                HorizontalAlignment = HorizontalAlignment.Left;
                VerticalAlignment = VerticalAlignment.Center;
                break;
            case "Center":
                HorizontalAlignment = HorizontalAlignment.Center;
                VerticalAlignment = VerticalAlignment.Center;
                break;
            case "CenterRight":
                HorizontalAlignment = HorizontalAlignment.Right;
                VerticalAlignment = VerticalAlignment.Center;
                break;
            case "BottomLeft":
                HorizontalAlignment = HorizontalAlignment.Left;
                VerticalAlignment = VerticalAlignment.Bottom;
                break;
            case "BottomCenter":
                HorizontalAlignment = HorizontalAlignment.Center;
                VerticalAlignment = VerticalAlignment.Bottom;
                break;
            case "BottomRight":
                HorizontalAlignment = HorizontalAlignment.Right;
                VerticalAlignment = VerticalAlignment.Bottom;
                break;
        }
    }
}