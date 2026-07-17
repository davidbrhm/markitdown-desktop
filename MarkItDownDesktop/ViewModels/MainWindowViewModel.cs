using System;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MarkItDownDesktop.Models;

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
}