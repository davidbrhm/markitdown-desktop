using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using MarkItDownDesktop.Models;
using MarkItDownDesktop.ViewModels;

namespace MarkItDownDesktop.Views.Components;

public partial class OutboxView : UserControl
{
    private Point _dragStartPoint;
    private bool _isDragging;
    private ConvertedFile? _pressedItem;
    private bool _isPressedItemAlreadySelected;

    public OutboxView()
    {
        InitializeComponent();
        OutputListBox.AddHandler(PointerPressedEvent, OnOutputListBoxPointerPressed, RoutingStrategies.Tunnel);
    }

    public void SelectAll() => OutputListBox.SelectAll();

    public IEnumerable<ConvertedFile> GetSelectedFiles()
    {
        return OutputListBox.SelectedItems?.Cast<ConvertedFile>() ?? Array.Empty<ConvertedFile>();
    }

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
                if (!hasModifiers) e.Handled = true;
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

    private async Task<IEnumerable<IStorageItem>> GetStorageItemsAsync(IEnumerable<string> filePaths)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null) return Array.Empty<IStorageItem>();

        var items = new List<IStorageItem>();
        foreach (var path in filePaths)
        {
            var uri = new Uri(path);
            var storageFile = await topLevel.StorageProvider.TryGetFileFromPathAsync(uri);
            if (storageFile is not null) items.Add(storageFile);
        }

        return items;
    }

    private async void OnOutputListBoxSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                var selectedItems = OutputListBox.SelectedItems?
                    .Cast<ConvertedFile>()
                    .ToList() ?? new List<ConvertedFile>();

                await viewModel.UpdatePreviewAsync(selectedItems);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}