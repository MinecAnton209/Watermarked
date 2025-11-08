using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Watermarked.Views;

public partial class DialogView : Window
{
    public DialogView()
    {
        InitializeComponent();
    }

    private void OkButton_OnClick(object? sender, RoutedEventArgs e)
    {
        this.Close();
    }
}