using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using MarkItDownDesktop.ViewModels;

namespace MarkItDownDesktop.Views.Components;

public partial class InboxView : UserControl
{
    public InboxView()
    {
        InitializeComponent();
    }

    private void OnDragOver(object? sender, DragEventArgs e)
    {
        if (e.DataTransfer.Contains(DataFormat.File))
            e.DragEffects = DragDropEffects.Copy;
        else
            e.DragEffects = DragDropEffects.None;
    }

    private async void OnDrop(object? sender, DragEventArgs e)
    {
        try
        {
            var storageItems = e.DataTransfer.TryGetFiles();
            if (storageItems is null) return;

            var filePaths = storageItems
                .Select(file => file.Path.LocalPath)
                .Where(path => !string.IsNullOrEmpty(path))
                .ToArray();

            if (filePaths.Length > 0 && DataContext is MainWindowViewModel viewModel)
            {
                await viewModel.ImportFilesAsync(filePaths);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private async void OnDragDropBorderPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        try
        {
            var pointerProperties = e.GetCurrentPoint(this).Properties;
            if (!pointerProperties.IsLeftButtonPressed) return;

            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel is null) return;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Choose files",
                AllowMultiple = true
            });

            if (files.Count > 0)
            {
                string[] filePaths = files
                    .Select(file => file.Path.LocalPath)
                    .Where(path => !string.IsNullOrEmpty(path))
                    .ToArray();

                if (DataContext is MainWindowViewModel viewModel)
                {
                    await viewModel.ImportFilesAsync(filePaths);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}