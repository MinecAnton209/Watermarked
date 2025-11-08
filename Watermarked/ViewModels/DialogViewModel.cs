using CommunityToolkit.Mvvm.ComponentModel;

namespace Watermarked.ViewModels;

public partial class DialogViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _title = "Title";
    
    [ObservableProperty]
    private string _message = "Message";
}