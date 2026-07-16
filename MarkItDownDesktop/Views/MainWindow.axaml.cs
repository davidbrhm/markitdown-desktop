using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Platform.Storage;
using MarkItDownDesktop.ViewModels;

namespace MarkItDownDesktop.Views;

// TODO: exception handling
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

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
        var selectedFiles = OutboxComponent.GetSelectedFiles().ToList();
        if (selectedFiles.Count > 0)
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
                        OutboxComponent.SelectAll();
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

    #endregion
}