using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using Watermarked.ViewModels;

namespace Watermarked.Views;

public partial class FileListView : UserControl
{
    private MainViewModel? Vm => DataContext as MainViewModel;

    private int _dragDepth = 0;
    public event Action<string, string>? ShowErrorRequested;

    public FileListView()
    {
        InitializeComponent();
        
        this.AttachedToVisualTree += OnAttachedToVisualTree;
    }
    
    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (Vm is not null)
        {
            Vm.ShowErrorRequested += async (title, message) =>
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

    private void DropZone_DragEnter(object? sender, DragEventArgs e)
    {
        if (!e.Data.Contains(DataFormats.FileNames))
            return;

        _dragDepth++;
        DropIndicator.IsVisible = true;
    }

    private void DropZone_DragLeave(object? sender, DragEventArgs e)
    {
        _dragDepth--;
        if (_dragDepth <= 0)
        {
            DropIndicator.IsVisible = false;
            _dragDepth = 0;
        }
    }

    private void DropZone_DragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects = e.Data.Contains(DataFormats.FileNames)
            ? DragDropEffects.Copy
            : DragDropEffects.None;
    }

    private void DropZone_Drop(object? sender, DragEventArgs e)
    {
        _dragDepth = 0;
        DropIndicator.IsVisible = false;

        if (Vm is null || !e.Data.Contains(DataFormats.FileNames)) return;

        var fileNames = e.Data.GetFileNames();
        if (fileNames is null) return;
        
        Vm.AddDroppedFiles(fileNames);
    }
}