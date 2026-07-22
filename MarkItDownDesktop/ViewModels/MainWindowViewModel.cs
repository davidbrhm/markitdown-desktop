using System;
using System.Linq;
using MarkItDownDesktop.Models;
using MarkItDownDesktop.Services;

namespace MarkItDownDesktop.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly WorkspaceService _workspaceService;

    public MainWindowViewModel()
    {
        _workspaceService = new WorkspaceService();

        SyncWithWorkspace();
    }

    private void SyncWithWorkspace()
    {
        // FileSystemWatcher
        var inboxPaths = _workspaceService.GetInboxFiles().ToArray();
        var outboxPaths = _workspaceService.GetOutboxFiles().ToArray();

        if (inboxPaths.Length != outboxPaths.Length) throw new Exception("Inbox and Outbox counts do not match!");

        InboxFiles.Clear();
        ConvertedFiles.Clear();

        foreach (string path in inboxPaths)
            InboxFiles.Add(ConvertedFile.FromPath(path));

        foreach (string path in outboxPaths)
            ConvertedFiles.Add(ConvertedFile.FromPath(path));

        OnPropertyChanged(nameof(HasFiles));
    }
}