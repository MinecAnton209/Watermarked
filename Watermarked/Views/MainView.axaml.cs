using Avalonia.Controls;
using Watermarked.ViewModels;

namespace Watermarked.Views;

public partial class MainView : UserControl
{
    
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