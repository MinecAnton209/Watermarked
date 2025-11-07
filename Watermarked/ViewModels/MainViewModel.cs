using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Watermarked.Models;

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
    
    #endregion

    #region Computed Properties
    
    public bool IsImageSelected => SelectedFile is not null &&
                                   new[] { ".png", ".jpg", ".jpeg", ".bmp", ".webp" }
                                       .Contains(Path.GetExtension(SelectedFile).ToLowerInvariant());
    
    #endregion

    #region Collections

    public ObservableCollection<string> Files { get; } = new();

    #endregion
    
    #region Child ViewModels

    public WatermarkSettingsViewModel Settings { get; } = new();

    #endregion
    
    #region Templates Logic

    private readonly string _templatesPath;

    public ObservableCollection<WatermarkTemplate> Templates { get; } = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteTemplateCommand))]
    private WatermarkTemplate? _selectedTemplate;

    [ObservableProperty]
    private string _newTemplateName = "My Awesome Style";
    
    #endregion
    
    #region Constructor

    public MainViewModel()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _templatesPath = Path.Combine(appDataPath, "Watermarked", "Templates");
        Directory.CreateDirectory(_templatesPath);
        
        LoadTemplatesFromDisk();
    }
    
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

    partial void OnSelectedTemplateChanged(WatermarkTemplate? value)
    {
        if (value is null) return;

        Settings.WatermarkText = value.WatermarkText;
        Settings.FontSize = value.FontSize;
        Settings.TextColor = value.TextColor;
        Settings.IsTextWrappingEnabled = value.IsTextWrappingEnabled;
        Settings.Opacity = value.Opacity;
        
        var newFont = FontManager.Current.SystemFonts.FirstOrDefault(f => f.Name == value.FontFamilyName);
        Settings.SelectedFontFamily = newFont ?? FontManager.Current.SystemFonts.First();

        Settings.Position.HorizontalAlignment = value.HorizontalAlignment;
        Settings.Position.VerticalAlignment = value.VerticalAlignment;
        Settings.Position.Margin = new Thickness(0);
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
        Patterns = FilePickerFileTypes.ImageAll.Patterns?.Concat(VideoAll.Patterns ?? Enumerable.Empty<string>()).ToArray(),
        MimeTypes = FilePickerFileTypes.ImageAll.MimeTypes?.Concat(VideoAll.MimeTypes ?? Enumerable.Empty<string>()).ToArray(),
        AppleUniformTypeIdentifiers = FilePickerFileTypes.ImageAll.AppleUniformTypeIdentifiers?.Concat(VideoAll.AppleUniformTypeIdentifiers ?? Enumerable.Empty<string>()).ToArray()
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
            FileTypeFilter = new[] { MediaAll, FilePickerFileTypes.All }
        };

        var result = await topLevel.StorageProvider.OpenFilePickerAsync(filePickerOptions);

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
    
    [RelayCommand]
    private void SaveTemplate()
    {
        var newTemplate = new WatermarkTemplate
        {
            Name = NewTemplateName,
            WatermarkText = Settings.WatermarkText,
            FontSize = Settings.FontSize,
            TextColor = Settings.TextColor,
            IsTextWrappingEnabled = Settings.IsTextWrappingEnabled,
            Opacity = Settings.Opacity,
            FontFamilyName = Settings.SelectedFontFamily.Name,
            HorizontalAlignment = Settings.Position.HorizontalAlignment,
            VerticalAlignment = Settings.Position.VerticalAlignment
        };

        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(newTemplate, options);
        var filePath = Path.Combine(_templatesPath, $"{SanitizeFileName(newTemplate.Name)}.json");
        File.WriteAllText(filePath, json);

        LoadTemplatesFromDisk();
    }
    
    [RelayCommand(CanExecute = nameof(CanDeleteTemplate))]
    private void DeleteTemplate()
    {
        if (SelectedTemplate is null) return;
        
        var filePath = Path.Combine(_templatesPath, $"{SanitizeFileName(SelectedTemplate.Name)}.json");
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        
        Templates.Remove(SelectedTemplate);
    }
    
    private bool CanDeleteTemplate() => SelectedTemplate is not null;
    
    #endregion
    
    #region Private Methods

    private void LoadTemplatesFromDisk()
    {
        Templates.Clear();
        if (!Directory.Exists(_templatesPath)) return;
        
        var templateFiles = Directory.GetFiles(_templatesPath, "*.json");
        foreach (var file in templateFiles)
        {
            try
            {
                var json = File.ReadAllText(file);
                var template = JsonSerializer.Deserialize<WatermarkTemplate>(json);
                if (template is not null)
                {
                    Templates.Add(template);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to load template {file}: {e.Message}");
            }
        }
    }

    private string SanitizeFileName(string name)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(c, '_');
        }
        return name;
    }
    
    #endregion
}