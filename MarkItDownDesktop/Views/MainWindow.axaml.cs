using System;
using System.Collections.Generic;
using Avalonia.Controls;
using MarkItDownDesktop.ViewModels;
using Avalonia.Input;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Input.Platform;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using MarkItDownDesktop.Models;

namespace MarkItDownDesktop.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    #region Inbox

    private void OnDragOver(object? sender, DragEventArgs e)
    {
        if (e.DataTransfer.Contains(DataFormat.File))
            e.DragEffects = DragDropEffects.Copy;
        else
            e.DragEffects = DragDropEffects.None;
    }

    private async void OnDrop(object? sender, DragEventArgs e)
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

    private async void OnDragDropBorderPointerPressed(object? sender, PointerPressedEventArgs e)
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

    #endregion

    #region Keyboard Shortcuts

    private async Task HandlePaste()
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel?.Clipboard is null) return;

        var clipboardItems = await topLevel.Clipboard.TryGetFilesAsync();
        if (clipboardItems is null) return;

        string[] filePaths = clipboardItems
            .Select(item => item.Path.LocalPath)
            .Where(path => !string.IsNullOrEmpty(path))
            .ToArray();

        if (filePaths.Length > 0 && DataContext is MainWindowViewModel viewModel)
        {
            await viewModel.ImportFilesAsync(filePaths);
        }
    }

    private async Task HandleCopy()
    {
        if (OutputListBox.SelectedItems?.Cast<ConvertedFile>().ToList() is { Count: > 0 } selectedFiles)
        {
            var filePaths = selectedFiles.Select(f => f.Path).ToArray();
            var storageItems = await GetStorageItemsAsync(filePaths);

            // TODO: 11.1
            var dataObject = new DataObject();
            dataObject.Set(DataFormats.Files, storageItems);

            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel?.Clipboard is not null)
            {
                await topLevel.Clipboard.SetDataObjectAsync(dataObject);
            }
        }
    }

    private async void OnWindowKeyDown(object? sender, KeyEventArgs e)
    {
        bool isModifierPressed = e.KeyModifiers.HasFlag(KeyModifiers.Meta) ||
                                 e.KeyModifiers.HasFlag(KeyModifiers.Control);

        if (isModifierPressed)
        {
            var key = e.Key;
            switch (key)
            {
                case Key.C:
                    await HandleCopy(); break;
                case Key.V:
                    await HandlePaste(); break;
            }
        }
    }

    #endregion


    #region Outbox

    private void OnOutputListBoxPointerMoved(object? sender, PointerEventArgs e)
    {
        throw new NotImplementedException();
    }

    private async void OnOutputListBoxPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private async Task StartDragOut(PointerEventArgs e)
    {
        throw new NotImplementedException();
    }

    #endregion

    }

    #endregion
}