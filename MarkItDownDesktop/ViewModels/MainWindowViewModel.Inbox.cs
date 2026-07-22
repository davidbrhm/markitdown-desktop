using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using MarkItDownDesktop.Models;

namespace MarkItDownDesktop.ViewModels;

public partial class MainWindowViewModel
{
    public ObservableCollection<ConvertedFile> InboxFiles { get; } = [];
    public bool HasFiles => ConvertedFiles.Count > 0;

    public async Task ImportFilesAsync(string[] filePaths)
    {
        await _workspaceService.ImportToInboxAsync(filePaths);
        Dispatcher.UIThread.Post(SyncWithWorkspace);
    }

    [RelayCommand]
    private void ClearWorkspace()
    {
        _workspaceService.ClearWorkspace();

        SyncWithWorkspace();
        OnPropertyChanged(nameof(HasFiles));
    }
}