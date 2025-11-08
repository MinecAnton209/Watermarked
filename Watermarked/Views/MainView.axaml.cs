using Avalonia.Controls;
using Watermarked.ViewModels;

namespace Watermarked.Views;

public partial class MainView : UserControl
{
    private int _dragDepth = 0;
    
    public MainView()
    {
        InitializeComponent();
    }
    
    private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            vm.UpdateLayout(e.NewSize.Width);
        }
    }
}