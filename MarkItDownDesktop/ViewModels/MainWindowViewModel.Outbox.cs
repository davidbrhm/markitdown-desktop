using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MarkItDownDesktop.Models;

namespace MarkItDownDesktop.ViewModels;

public partial class MainWindowViewModel
{
    public ObservableCollection<ConvertedFile> ConvertedFiles { get; } = [];

    [ObservableProperty] private bool _isCodeViewActive = false;
    [ObservableProperty] private ConvertedFile? _selectedFile;
    [ObservableProperty] private string _codeViewText = string.Empty;

    [RelayCommand]
    private void SelectFileView() => IsCodeViewActive = false;

    [RelayCommand]
    private void SelectCodeView() => IsCodeViewActive = true;

    partial void OnSelectedFileChanged(ConvertedFile? value)
    {
        return;
        throw new NotImplementedException();
    }

    public async Task UpdatePreviewAsync(IList<ConvertedFile> selectedFiles)
    {
        if (selectedFiles.Count != 1)
        {
            SelectFileView();
            SelectedFile = null;
            CodeViewText = string.Empty;
            return;
        }

        /* merge after MVP
        if (selectedFiles.Count > 1)
        */

        var targetFile = selectedFiles[0];
        SelectedFile = targetFile;

        if (!File.Exists(targetFile.Path))
        {
            CodeViewText = "The file does not exist";
            return;
        }

        try
        {
            CodeViewText = await File.ReadAllTextAsync(targetFile.Path);
        }
        catch (Exception ex)
        {
            CodeViewText = $"Error reading file:\n{ex.Message}";
        }
    }
}