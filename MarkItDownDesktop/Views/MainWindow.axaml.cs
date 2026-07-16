using System;
using System.Collections.Generic;
using Avalonia.Controls;
using MarkItDownDesktop.ViewModels;
using Avalonia.Input;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using MarkItDownDesktop.Models;

namespace MarkItDownDesktop.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        OutputListBox.AddHandler(PointerPressedEvent, OnOutputListBoxPointerPressed, RoutingStrategies.Tunnel);
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

    #endregion

    #region Keyboard Shortcuts

    private async Task HandlePasteAsync()
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

    private async Task HandleCopyAsync()
    {
        if (OutputListBox.SelectedItems?.Cast<ConvertedFile>().ToList() is { Count: > 0 } selectedFiles)
        {
            var filePaths = selectedFiles.Select(f => f.Path).ToArray();
            var storageItems = await GetStorageItemsAsync(filePaths);

            var dataObject = new DataTransfer();

            foreach (var storageItem in storageItems)
            {
                dataObject.Add(DataTransferItem.CreateFile(storageItem));
            }

            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel?.Clipboard is not null)
            {
                await topLevel.Clipboard.SetDataAsync(dataObject);
            }
        }
    }

    private async void OnWindowKeyDown(object? sender, KeyEventArgs e)
    {
        try
        {
            bool isModifierPressed = e.KeyModifiers.HasFlag(KeyModifiers.Meta) ||
                                     e.KeyModifiers.HasFlag(KeyModifiers.Control);

            if (isModifierPressed)
            {
                switch (e.Key)
                {
                    case Key.C:
                        await HandleCopyAsync();
                        break;
                    case Key.V:
                        await HandlePasteAsync();
                        break;
                    case Key.A:
                        OutputListBox.SelectAll();
                        e.Handled = true;
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    #endregion

    #region Outbox

    #region DragOut

    private Point _dragStartPoint;
    private bool _isDragging;
    private ConvertedFile? _pressedItem;
    private bool _isPressedItemAlreadySelected;

    private void OnOutputListBoxPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var properties = e.GetCurrentPoint(this).Properties;
        if (!properties.IsLeftButtonPressed) return;

        _dragStartPoint = e.GetPosition(this);
        _isDragging = false;
        _pressedItem = null;
        _isPressedItemAlreadySelected = false;

        Visual? clickedVisual = e.Source as Visual;
        ListBoxItem? listBoxItem = clickedVisual?.FindAncestorOfType<ListBoxItem>();

        if (listBoxItem?.DataContext is ConvertedFile clickedFile)
        {
            _pressedItem = clickedFile;
            var selectedFiles = OutputListBox.SelectedItems?.Cast<ConvertedFile>().ToList();

            if (selectedFiles is not null && selectedFiles.Contains(clickedFile))
            {
                _isPressedItemAlreadySelected = true;

                bool hasModifiers = e.KeyModifiers.HasFlag(KeyModifiers.Control) ||
                                    e.KeyModifiers.HasFlag(KeyModifiers.Meta) ||
                                    e.KeyModifiers.HasFlag(KeyModifiers.Shift);

                if (!hasModifiers)
                {
                    e.Handled = true;
                }
            }
        }
    }

    private void OnOutputListBoxPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_isDragging && _pressedItem is not null && _isPressedItemAlreadySelected)
        {
            bool hasModifiers = e.KeyModifiers.HasFlag(KeyModifiers.Control) ||
                                e.KeyModifiers.HasFlag(KeyModifiers.Meta) ||
                                e.KeyModifiers.HasFlag(KeyModifiers.Shift);
            if (!hasModifiers)
            {
                OutputListBox.SelectedItems?.Clear();
                OutputListBox.SelectedItems?.Add(_pressedItem);
            }
        }

        _pressedItem = null;
        _isPressedItemAlreadySelected = false;
        _isDragging = false;
    }

    private async void OnOutputListBoxPointerMoved(object? sender, PointerEventArgs e)
    {
        try
        {
            var point = e.GetCurrentPoint(this);

            if (!point.Properties.IsLeftButtonPressed || _isDragging) return;

            var currentPosition = e.GetPosition(this);
            var diff = currentPosition - _dragStartPoint;

            if (Math.Abs(diff.X) > 3 || Math.Abs(diff.Y) > 3)
            {
                _isDragging = true;
                await StartDragOut(e);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private async Task StartDragOut(PointerEventArgs e)
    {
        try
        {
            Visual? clickedVisual = e.Source as Visual;
            ListBoxItem? listBoxItem = clickedVisual?.FindAncestorOfType<ListBoxItem>();

            if (listBoxItem is null) return;

            var selectedFiles = OutputListBox.SelectedItems?.Cast<ConvertedFile>().ToList();
            var clickedFile = listBoxItem.DataContext as ConvertedFile;

            if (clickedFile is null) return;

            if (selectedFiles is null || !selectedFiles.Contains(clickedFile))
            {
                selectedFiles = new List<ConvertedFile> { clickedFile };
            }

            if (selectedFiles.Count == 0) return;

            var filePaths = selectedFiles.Select(f => f.Path).ToArray();
            var storageItems = await GetStorageItemsAsync(filePaths);

            var dataObject = new DataTransfer();
            foreach (var storageItem in storageItems)
            {
                dataObject.Add(DataTransferItem.CreateFile(storageItem));
            }

            await DragDrop.DoDragDropAsync(e, dataObject, DragDropEffects.Copy);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        finally
        {
            _isDragging = false;
        }
    }

    #endregion

    #endregion

    #region Helpers

    private async Task<IEnumerable<IStorageItem>> GetStorageItemsAsync(IEnumerable<string> filePaths)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null) return Array.Empty<IStorageItem>();

        var items = new List<IStorageItem>();
        foreach (var path in filePaths)
        {
            var uri = new Uri(path);
            var storageFile = await topLevel.StorageProvider.TryGetFileFromPathAsync(uri);

            if (storageFile is not null)
            {
                items.Add(storageFile);
            }
        }

        return items;
    }

    #endregion
}