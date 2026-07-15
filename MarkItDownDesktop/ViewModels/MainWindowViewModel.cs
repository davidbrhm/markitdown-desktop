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

        LoadExistingFiles();
    }

    #region LEFT PANEL: Import / Drag & Drop Zone

    public ObservableCollection<ConvertedFile> ConvertedFiles { get; } = new();
    public bool HasFiles => ConvertedFiles.Count > 0;

    private void LoadExistingFiles()
    {
        if (!Directory.Exists(_workspaceDirectory)) return;

        string[] files = Directory.GetFiles(_workspaceDirectory);
        foreach (string path in files)
        {
            string fileName = Path.GetFileName(path);
            if (fileName.StartsWith('.')) continue; // ignore hidden files

            ConvertedFile newFile = ConvertedFile.FromPath(path);
            ConvertedFiles.Add(newFile);
        }
    }

    public async Task ImportFilesAsync(string[] filePaths)
    {
        await Task.Run(() =>
        {
            foreach (string path in filePaths)
            {
                try
                {
                    string destinationPath = CopyToWorkspace(path);
                    string importTime = DateTime.Now.ToString("HH:mm:ss");

                    // TODO: MarkItDown

                    Dispatcher.UIThread.Post(() =>
                    {
                        ConvertedFile newFile = ConvertedFile.FromPath(destinationPath, importTime);
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

    [RelayCommand]
    private void ClearWorkspace()
    {
        try
        {
            if (!Directory.Exists(_workspaceDirectory)) return;

            string[] files = Directory.GetFiles(_workspaceDirectory);
            foreach (string filePath in files)
            {
                File.Delete(filePath);
            }

            ConvertedFiles.Clear();

            OnPropertyChanged(nameof(HasFiles));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    #endregion

    #region ViewToggleButtons

    [ObservableProperty] private bool _isCodeViewActive = false;

    [RelayCommand]
    private void SelectFileView() => IsCodeViewActive = false;

    [RelayCommand]
    private void SelectCodeView() => IsCodeViewActive = false; // TODO: fix switch bug

    #endregion

    #region RIGHT PANEL:  Export / Preview Zone

    [ObservableProperty] private ConvertedFile? _selectedFile;

    partial void OnSelectedFileChanged(ConvertedFile? value)
    {
        return;
        throw new NotImplementedException();
    }

    #endregion
}