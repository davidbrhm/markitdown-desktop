using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MarkItDownDesktop.Services;

public class WorkspaceService
{
    public string InboxPath { get; }
    public string OutboxPath { get; }

    public WorkspaceService()
    {
        var documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var baseWorkspacePath = Path.Combine(documentsFolder, "MarkItDownWorkspace");

        InboxPath = Path.Combine(baseWorkspacePath, "Inbox");
        OutboxPath = Path.Combine(baseWorkspacePath, "Outbox");

        EnsureDirectoriesExist();
    }

    private void EnsureDirectoriesExist()
    {
        if (!Directory.Exists(InboxPath))
            Directory.CreateDirectory(InboxPath);

        if (!Directory.Exists(OutboxPath))
            Directory.CreateDirectory(OutboxPath);
    }

    public IEnumerable<string> GetInboxFiles()
    {
        if (!Directory.Exists(InboxPath)) return [];

        // ignore hidden files
        return Directory.GetFiles(InboxPath).Where(path => !Path.GetFileName(path).StartsWith('.'));
    }

    public IEnumerable<string> GetOutboxFiles()
    {
        if (!Directory.Exists(OutboxPath)) return [];

        // ignore hidden files
        return Directory.GetFiles(OutboxPath).Where(path => !Path.GetFileName(path).StartsWith('.'));
    }

    public async Task ImportToInboxAsync(IEnumerable<string> filePaths)
    {
        await Task.Run(async () =>
        {
            foreach (var path in filePaths)
            {
                try
                {
                    string fileNameWithoutExt = Path.GetFileNameWithoutExtension(path);
                    string extension = Path.GetExtension(path);

                    string destInboxPath = Path.Combine(InboxPath, fileNameWithoutExt + extension);
                    string destOutboxPath = Path.Combine(OutboxPath, fileNameWithoutExt + ".md");

                    int counter = 1;
                    while (File.Exists(destInboxPath) || File.Exists(destOutboxPath))
                    {
                        string newName = $"{fileNameWithoutExt} ({counter})";
                        destInboxPath = Path.Combine(InboxPath, newName + extension);
                        destOutboxPath = Path.Combine(OutboxPath, newName + ".md");
                        counter++;
                    }

                    File.Copy(path, destInboxPath);

                    await ConvertToMarkdownAsync(path, destOutboxPath);
                }
                catch (Exception ex)
                {
                    // TODO: log
                    Console.WriteLine(ex);
                }
            }
        });
    }

    private async Task ConvertToMarkdownAsync(string sourcePath, string outboxPath)
    {
        // TODO: MarkItDown
        await Task.Run(() => File.Copy(sourcePath, outboxPath));
    }

    public void ClearWorkspace()
    {
        foreach (var file in Directory.GetFiles(InboxPath)) File.Delete(file);
        foreach (var file in Directory.GetFiles(OutboxPath)) File.Delete(file);
    }
}