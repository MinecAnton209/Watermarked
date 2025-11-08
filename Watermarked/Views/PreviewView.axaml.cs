using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Watermarked.ViewModels;

namespace Watermarked.Views;

public partial class PreviewView : UserControl
{
    private bool _isDragging;
    private Point _startPoint;
    
    private MainViewModel? Vm => DataContext as MainViewModel;
    
    public PreviewView()
    {
        InitializeComponent();
    }
    
    private void OnPreviewSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (Vm is not null)
        {
            Vm.Settings.Position.ContainerSize = e.NewSize;
        }
    }
        
    private void OnWatermarkSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (Vm is not null)
        {
            Vm.Settings.Position.WatermarkSize = e.NewSize;
        }
    }
    
    private void Watermark_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Control watermark || Vm is null) return;
        
        _isDragging = true;
        _startPoint = e.GetPosition(watermark);
        e.Pointer.Capture(watermark);

        Vm.Settings.Position.HorizontalAlignment = HorizontalAlignment.Left;
        
        Vm.Settings.Position.VerticalAlignment = VerticalAlignment.Top;
    }

    private void Watermark_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isDragging || Vm is null || sender is not Control watermark) return;
        
        var currentPoint = e.GetPosition(watermark.Parent as Visual);
        var position = Vm.Settings.Position;

        double newX = currentPoint.X - _startPoint.X;
        double newY = currentPoint.Y - _startPoint.Y;

        var watermarkSize = watermark.Bounds.Size;
        double maxX = position.ContainerSize.Width - watermarkSize.Width;
        if (newX < 0) newX = 0;
        if (newX > maxX) newX = maxX;
            
        double maxY = position.ContainerSize.Height - watermarkSize.Height;
        if (newY < 0) newY = 0;
        if (newY > maxY) newY = maxY;

        position.Margin = new Thickness(newX, newY, 0, 0);
    }

    private void Watermark_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_isDragging) return;
        _isDragging = false;
        e.Pointer.Capture(null);
    }
}