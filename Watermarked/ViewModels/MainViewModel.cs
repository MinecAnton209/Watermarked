using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Watermarked.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _greeting = "Welcome to Avalonia!";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsImageSelected))]
    private string? _selectedFile;

    [ObservableProperty]
    private Bitmap? _previewImage;

    public bool IsImageSelected => SelectedFile is not null &&
                                   new[] { ".png", ".jpg", ".jpeg", ".bmp" }
                                       .Contains(Path.GetExtension(SelectedFile).ToLower());

    public ObservableCollection<string> Files { get; } = new();

    partial void OnSelectedFileChanged(string? value)
    {
        PreviewImage?.Dispose();
        PreviewImage = null;

        if (value is not null && IsImageSelected)
        {
            try
            {
                PreviewImage = new Bitmap(value);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to load image: {e.Message}");
            }
        }
    }
    
    [RelayCommand]
    private void AddFiles()
    {
        var testImagePath = @"C:\Users\MinecAnton209\Pictures\Screenshots\Screenshot 2025-10-29 135548.png";
        
        Files.Add(testImagePath);
        SelectedFile = Files.LastOrDefault();
    }

    [RelayCommand]
    private void ClearList()
    {
        Files.Clear();
    }
}