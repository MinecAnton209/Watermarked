using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.VisualTree;
using Watermarked.ViewModels;

namespace Watermarked.Views;

public partial class MainView : UserControl
{
    private bool _isDragging;
    private Point _startPoint;
    
    public MainView()
    {
        InitializeComponent();
        
        this.AttachedToVisualTree += OnAttachedToVisualTree;
    }

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            vm.ShowErrorRequested += async (title, message) =>
            {
                var mainWindow = (Window)this.GetVisualRoot();

                var dialogVm = new DialogViewModel { Title = title, Message = message };
                var dialog = new DialogView
                {
                    DataContext = dialogVm
                };
            
                await dialog.ShowDialog(mainWindow);
            };
        
            this.AttachedToVisualTree -= OnAttachedToVisualTree;
        }
    }
    
    private void OnPreviewSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            vm.Settings.Position.ContainerSize = e.NewSize;
        }
    }
        
    private void OnWatermarkSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            vm.Settings.Position.WatermarkSize = e.NewSize;
        }
    }
    
    private void Watermark_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Control watermark || DataContext is not MainViewModel vm) return;
        
        _isDragging = true;
        _startPoint = e.GetPosition(watermark);
        e.Pointer.Capture(watermark);

        vm.Settings.Position.HorizontalAlignment = HorizontalAlignment.Left;
        vm.Settings.Position.VerticalAlignment = VerticalAlignment.Top;
    }

    private void Watermark_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isDragging || DataContext is not MainViewModel vm || sender is not Control watermark) return;
        
        var currentPoint = e.GetPosition(watermark.Parent as Visual);
        var position = vm.Settings.Position;

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