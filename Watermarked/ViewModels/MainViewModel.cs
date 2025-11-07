using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Watermarked.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    #region Observable Properties

    [ObservableProperty]
    private string _greeting = "Welcome to Avalonia!";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsImageSelected))]
    private string? _selectedFile;

    [ObservableProperty]
    private Bitmap? _previewImage;
    
    [ObservableProperty]
    private Size _previewSize;

    #endregion

    #region Computed Properties

    public bool IsImageSelected => SelectedFile is not null &&
                                   new[] { ".png", ".jpg", ".jpeg", ".bmp", ".webp" }
                                       .Contains(Path.GetExtension(SelectedFile).ToLowerInvariant());

    #endregion

    #region Collections

    public ObservableCollection<string> Files { get; } = new();
    
    public WatermarkSettingsViewModel Settings { get; } = new();

    #endregion

    #region Partial Methods (Hooks)

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

    #endregion

    #region File Type Definitions for Picker

    private static FilePickerFileType VideoAll { get; } = new("All Videos")
    {
        Patterns = new[] { "*.mp4", "*.mov", "*.avi", "*.mkv", "*.webm" },
        MimeTypes = new[] { "video/*" },
        AppleUniformTypeIdentifiers = new[] { "public.movie" }
    };

    private static FilePickerFileType MediaAll { get; } = new("All Media Files")
    {
        Patterns = FilePickerFileTypes.ImageAll.Patterns?
            .Concat(VideoAll.Patterns ?? Enumerable.Empty<string>()).ToArray(),
        MimeTypes = FilePickerFileTypes.ImageAll.MimeTypes?
            .Concat(VideoAll.MimeTypes ?? Enumerable.Empty<string>()).ToArray(),
        AppleUniformTypeIdentifiers = FilePickerFileTypes.ImageAll.AppleUniformTypeIdentifiers?
            .Concat(VideoAll.AppleUniformTypeIdentifiers ?? Enumerable.Empty<string>()).ToArray()
    };

    #endregion

    #region Commands

    [RelayCommand]
    private async Task AddFiles(TopLevel? topLevel)
    {
        if (topLevel is null) return;

        var filePickerOptions = new FilePickerOpenOptions
        {
            Title = Resources.Strings.OpenFilePicker_Title,
            AllowMultiple = true,
            FileTypeFilter = new[]
            {
                MediaAll,
                FilePickerFileTypes.All
            }
        };

        IReadOnlyList<IStorageFile> result = await topLevel.StorageProvider.OpenFilePickerAsync(filePickerOptions);

        if (result.Any())
        {
            foreach (var file in result)
            {
                Files.Add(file.Path.LocalPath);
            }

            SelectedFile = Files.LastOrDefault();
        }
    }

    [RelayCommand]
    private void ClearList()
    {
        Files.Clear();
    }

    #endregion
}