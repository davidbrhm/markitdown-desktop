using System;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MarkItDownDesktop.Models;

namespace MarkItDownDesktop.ViewModels;

public partial class MainWindowViewModel
{
    [ObservableProperty] private bool _isCodeViewActive = false;

    [RelayCommand]
    private void SelectFileView() => IsCodeViewActive = false;

    [RelayCommand]
    private void SelectCodeView() => IsCodeViewActive = false;



    [ObservableProperty] private ConvertedFile? _selectedFile;
    [ObservableProperty] private string _codeViewText = string.Empty;

    partial void OnSelectedFileChanged(ConvertedFile? value)
    {
        return;
        throw new NotImplementedException();
    }


}