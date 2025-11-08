using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using SkiaSharp;
using Watermarked.Models;

namespace Watermarked.ViewModels;

public record ShowErrorDialog(string Title, string Message);

public partial class MainViewModel : ViewModelBase
{
    public event Action<string, string>? ShowErrorRequested;
    
    #region Observable Properties

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

    #region Interactions

    public Interaction<ShowErrorDialog, Unit> ShowErrorInteraction { get; } = new();

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
                ShowErrorRequested?.Invoke(
                    Resources.Strings.ErrorDialog_Title,
                    $"Failed to load preview image:\n{e.Message}"
                );
            }
        }
    }

    partial void OnSelectedTemplateChanged(WatermarkTemplate? value)
    {
        if (value is null) return;
        
        Settings.WatermarkType = value.Type;
        Settings.WatermarkText = value.WatermarkText;
        Settings.FontSize = value.FontSize;
        Settings.TextColor = value.TextColor;
        Settings.IsTextWrappingEnabled = value.IsTextWrappingEnabled;
        Settings.Opacity = value.Opacity;
        Settings.ImagePath = value.ImagePath;
        Settings.ImageScale = value.ImageScale;
        
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
            Title = Resources.Strings.OpenFilePicker_Title, AllowMultiple = true,
            FileTypeFilter = new[] { MediaAll, FilePickerFileTypes.All }
        };
        var result = await topLevel.StorageProvider.OpenFilePickerAsync(filePickerOptions);
        if (result.Any())
        {
            foreach (var file in result) { Files.Add(file.Path.LocalPath); }
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
        bool isUpdating = SelectedTemplate != null;
        var templateToSave = isUpdating ? SelectedTemplate! : new WatermarkTemplate();

        templateToSave.Name = isUpdating ? SelectedTemplate!.Name : NewTemplateName;
        templateToSave.Type = Settings.WatermarkType;
        templateToSave.WatermarkText = Settings.WatermarkText;
        templateToSave.FontSize = Settings.FontSize;
        templateToSave.TextColor = Settings.TextColor;
        templateToSave.IsTextWrappingEnabled = Settings.IsTextWrappingEnabled;
        templateToSave.Opacity = Settings.Opacity;
        templateToSave.FontFamilyName = Settings.SelectedFontFamily.Name;
        templateToSave.ImagePath = Settings.ImagePath;
        templateToSave.ImageScale = Settings.ImageScale;
        templateToSave.HorizontalAlignment = Settings.Position.HorizontalAlignment;
        templateToSave.VerticalAlignment = Settings.Position.VerticalAlignment;
    
        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(templateToSave, options);
        var filePath = Path.Combine(_templatesPath, $"{SanitizeFileName(templateToSave.Name)}.json");
        File.WriteAllText(filePath, json);

        if (!isUpdating) { LoadTemplatesFromDisk(); }
    }
    
    [RelayCommand(CanExecute = nameof(CanDeleteTemplate))]
    private void DeleteTemplate()
    {
        if (SelectedTemplate is null) return;
        var filePath = Path.Combine(_templatesPath, $"{SanitizeFileName(SelectedTemplate.Name)}.json");
        if (File.Exists(filePath)) { File.Delete(filePath); }
        Templates.Remove(SelectedTemplate);
    }
    
    [RelayCommand(CanExecute = nameof(CanSaveFile))]
    private async Task SaveFile(TopLevel? topLevel)
    {
        if (topLevel is null || SelectedFile is null) return;
        
        var filePickerOptions = new FilePickerSaveOptions
        {
            Title = Resources.Strings.SaveFilePicker_Title,
            SuggestedFileName = $"{Path.GetFileNameWithoutExtension(SelectedFile)}_watermarked{Path.GetExtension(SelectedFile)}",
            DefaultExtension = Path.GetExtension(SelectedFile), FileTypeChoices = new[] { FilePickerFileTypes.ImageAll }
        };
        var result = await topLevel.StorageProvider.SaveFilePickerAsync(filePickerOptions);
        if (result is null) return;
        
        try
        {
            await using var inputStream = File.OpenRead(SelectedFile);
            using var originalBitmap = SKBitmap.Decode(inputStream);
            using var surface = SKSurface.Create(new SKImageInfo(originalBitmap.Width, originalBitmap.Height));
            var canvas = surface.Canvas;
            
            canvas.DrawBitmap(originalBitmap, 0, 0);

            switch (Settings.WatermarkType)
            {
                case WatermarkType.Text:
                    DrawTextWatermark(canvas, originalBitmap.Width, originalBitmap.Height); break;
                case WatermarkType.Image:
                    DrawImageWatermark(canvas, originalBitmap.Width, originalBitmap.Height); break;
            }
            
            using var image = surface.Snapshot();
            var format = Path.GetExtension(SelectedFile).ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => SKEncodedImageFormat.Jpeg, ".png" => SKEncodedImageFormat.Png,
                ".webp" => SKEncodedImageFormat.Webp, _ => SKEncodedImageFormat.Png
            };
            using var data = image.Encode(format, 100);
            await using var outputStream = await result.OpenWriteAsync();
            data.SaveTo(outputStream);
        }
        catch (Exception e)
        {
            ShowErrorRequested?.Invoke(
                Resources.Strings.ErrorDialog_Title, 
                $"{Resources.Strings.SaveFileError_Message}\n{e.Message}"
            );
        }
    }
    
    private bool CanSaveFile() => SelectedFile is not null && IsImageSelected;
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
                if (template is not null) { Templates.Add(template); }
            }
            catch (Exception e)
            {
                ShowErrorRequested?.Invoke(
                    Resources.Strings.ErrorDialog_Title,
                    $"Failed to load template '{Path.GetFileName(file)}':\n{e.Message}"
                );
            }
        }
    }

    private string SanitizeFileName(string name)
    {
        foreach (char c in Path.GetInvalidFileNameChars()) { name = name.Replace(c, '_'); }
        return name;
    }
    
    private void DrawTextWatermark(SKCanvas canvas, int width, int height)
    {
        using var paint = new SKPaint
        {
            Color = new SKColor(Settings.TextColor.R, Settings.TextColor.G, Settings.TextColor.B, (byte)(Settings.Opacity * 255)),
            IsAntialias = true, Typeface = SKTypeface.FromFamilyName(Settings.SelectedFontFamily.Name, SKFontStyle.Normal),
            TextSize = (float)Settings.FontSize,
        };
        var text = Settings.WatermarkText;
        var lines = new List<string>();
        float maxWidth = width;
        if (Settings.IsTextWrappingEnabled)
        {
            string[] words = text.Split(' ');
            var currentLine = "";
            foreach (var word in words)
            {
                var testLine = string.IsNullOrEmpty(currentLine) ? word : $"{currentLine} {word}";
                if (paint.MeasureText(testLine) > maxWidth) { lines.Add(currentLine); currentLine = word; }
                else { currentLine = testLine; }
            }
            lines.Add(currentLine);
        }
        else { lines.Add(text); }
        float totalTextHeight = lines.Count * paint.TextSize;
        float maxTextWidth = lines.Any() ? lines.Max(line => paint.MeasureText(line)) : 0;
        var position = CalculateRenderPosition(width, height, maxTextWidth, totalTextHeight);
        float currentY = position.Y;
        foreach (var line in lines)
        {
            float lineX = position.X;
            if (Settings.Position.HorizontalAlignment == Avalonia.Layout.HorizontalAlignment.Center)
            {
                float lineWidth = paint.MeasureText(line);
                lineX += (maxTextWidth - lineWidth) / 2f;
            }
            var textBounds = new SKRect();
            paint.MeasureText(line, ref textBounds);
            canvas.DrawText(line, lineX - textBounds.Left, currentY - textBounds.Top, paint);
            currentY += paint.TextSize;
        }
    }
    
    private void DrawImageWatermark(SKCanvas canvas, int width, int height)
    {
        if (Settings.ImagePath is null || !File.Exists(Settings.ImagePath)) return;
        using var watermarkBitmap = SKBitmap.Decode(Settings.ImagePath);
        if (watermarkBitmap is null) return;
        float newWidth = width * (float)(Settings.ImageScale / 100.0);
        float scale = newWidth / watermarkBitmap.Width;
        float newHeight = watermarkBitmap.Height * scale;
        var position = CalculateRenderPosition(width, height, newWidth, newHeight);
        var destRect = SKRect.Create(position.X, position.Y, newWidth, newHeight);
        using var paint = new SKPaint { Color = new SKColor(255, 255, 255, (byte)(Settings.Opacity * 255)) };
        canvas.DrawBitmap(watermarkBitmap, destRect, paint);
    }
    
    private SKPoint CalculateRenderPosition(float canvasWidth, float canvasHeight, float watermarkWidth, float watermarkHeight)
    {
        float x = 0, y = 0;
        switch (Settings.Position.HorizontalAlignment)
        {
            case Avalonia.Layout.HorizontalAlignment.Center: x = (canvasWidth - watermarkWidth) / 2f; break;
            case Avalonia.Layout.HorizontalAlignment.Right: x = canvasWidth - watermarkWidth; break;
        }
        switch (Settings.Position.VerticalAlignment)
        {
            case Avalonia.Layout.VerticalAlignment.Center: y = (canvasHeight - watermarkHeight) / 2f; break;
            case Avalonia.Layout.VerticalAlignment.Bottom: y = canvasHeight - watermarkHeight; break;
        }
        x += (float)Settings.Position.Margin.Left;
        y += (float)Settings.Position.Margin.Top;
        if (x < 0) x = 0; if (y < 0) y = 0;
        if (x > canvasWidth - watermarkWidth) x = canvasWidth - watermarkWidth;
        if (y > canvasHeight - watermarkHeight) y = canvasHeight - watermarkHeight;
        return new SKPoint(x, y);
    }
    
    #endregion
}