using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MarkItDownDesktop.Models;
using Avalonia.Threading;

namespace MarkItDownDesktop.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly string _workspaceDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Workspace");

    public MainWindowViewModel()
    {
        if (!Directory.Exists(_workspaceDirectory))
        {
            Directory.CreateDirectory(_workspaceDirectory);
        }
    }

    public bool HasFiles => ConvertedFiles.Count > 0;

    #region ViewToggleButtons

    [ObservableProperty] private bool _isCodeViewActive = false;

    [RelayCommand]
    private void SelectFileView() => IsCodeViewActive = false;

    [RelayCommand]
    private void SelectCodeView() => IsCodeViewActive = true;

    #endregion

    #region FileConverter

    public ObservableCollection<ConvertedFile> ConvertedFiles { get; } = new();

    public async Task ImportFilesAsync(string[] filePaths)
    {
        await Task.Run(() =>
        {
            foreach (var path in filePaths)
            {
                try
                {
                    string destinationPath = CopyToWorkspace(path);

                    string formattedSize = GetFormattedFileSize(destinationPath);
                    string fileName = Path.GetFileName(destinationPath);
                    string importTime = DateTime.Now.ToString("HH:mm:ss");

                    // TODO: MarkItDown

                    Dispatcher.UIThread.Post(() =>
                    {
                        var newFile = new ConvertedFile(
                            fileName,
                            destinationPath,
                            formattedSize,
                            importTime
                        );
                        ConvertedFiles.Add(newFile);

                        OnPropertyChanged(nameof(HasFiles));
                    });
                }
                catch (Exception ex)
                {
                    // TODO: log
                    Console.WriteLine(ex);
                }
            }
        });
    }

    private string CopyToWorkspace(string sourcePath)
    {
        string fileNameWithoutExt = Path.GetFileNameWithoutExtension(sourcePath);
        string extension = Path.GetExtension(sourcePath);
        string destPath = Path.Combine(_workspaceDirectory, fileNameWithoutExt + extension);

        int counter = 1;
        while (File.Exists(destPath))
        {
            var newFileName = $"{fileNameWithoutExt} ({counter}){extension}";
            destPath = Path.Combine(_workspaceDirectory, newFileName);
            counter++;
        }

        File.Copy(sourcePath, destPath);

        return destPath;
    }

    #endregion

    #region Helpers

    private string GetFormattedFileSize(string filePath)
    {
        var fileInfo = new FileInfo(filePath);
        double bytes = fileInfo.Length;

        if (bytes < 1024)
            return $"{bytes} B";

        if (bytes < 1024 * 1024)
            return $"{(bytes / 1024.0):F1} KB";

        return $"{(bytes / (1024.0 * 1024.0)):F1} MB";
    }

    #endregion
}