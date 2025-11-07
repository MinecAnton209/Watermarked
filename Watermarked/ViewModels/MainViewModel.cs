using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Watermarked.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _greeting = "Welcome to Avalonia!";

    public ObservableCollection<string> Files { get; } = new();

    [RelayCommand]
    private void AddFiles()
    {
        Files.Add($"C:/Test/image_{Files.Count + 1}.jpg");
        Files.Add($"C:/Test/video_{Files.Count + 1}.mp4");
    }

    [RelayCommand]
    private void ClearList()
    {
        Files.Clear();
    }
}