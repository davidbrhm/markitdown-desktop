using Avalonia.Controls;
using MarkItDownDesktop.ViewModels;
using Avalonia.Input;
using System.Linq;

namespace MarkItDownDesktop.Views;

public partial class MainWindow : Window
{
    public MainWindow()
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
        var storageItems = e.DataTransfer.TryGetFiles();

        if (storageItems != null)
        {
            var filePaths = storageItems
                .Select(file => file.Path.LocalPath)
                .Where(path => !string.IsNullOrEmpty(path))
                .ToArray();

            if (filePaths.Length > 0 && DataContext is MainWindowViewModel viewModel)
            {
                await viewModel.ImportFilesAsync(filePaths);
            }
        }
    }
}