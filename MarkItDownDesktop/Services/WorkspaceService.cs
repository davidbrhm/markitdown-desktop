using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CliWrap;

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

        return Directory.GetFiles(InboxPath).Where(path => !Path.GetFileName(path).StartsWith('.'));
    }

    public IEnumerable<string> GetOutboxFiles()
    {
        if (!Directory.Exists(OutboxPath)) return [];

        return Directory.GetFiles(OutboxPath).Where(path => !Path.GetFileName(path).StartsWith('.'));
    }

    public void ClearWorkspace()
    {
        foreach (var file in Directory.GetFiles(InboxPath)) File.Delete(file);
        foreach (var file in Directory.GetFiles(OutboxPath)) File.Delete(file);
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
        try
        {
            var stderrBuffer = new System.Text.StringBuilder();

            var targetFilePath = GetMarkItDownExecutablePath();
            var result = await Cli.Wrap(targetFilePath)
                .WithArguments(sourcePath)
                .WithStandardOutputPipe(PipeTarget.ToFile(outboxPath))
                .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stderrBuffer))
                .WithValidation(CommandResultValidation.None)
                .ExecuteAsync();

            // handle non-zero exit codes by writing the error details to the output file
            if (result.ExitCode != 0)
            {
                string errorMsg = $"# Error during conversion\n\n" +
                                  $"**File:** `{sourcePath}`\n\n" +
                                  $"**Exit Code:** `{result.ExitCode}`\n\n" +
                                  $"**Python Error Details:**\n```text\n{stderrBuffer.ToString().Trim()}\n```";

                await File.WriteAllTextAsync(outboxPath, errorMsg);
            }
        }
        catch (Exception ex)
        {
            string errorMsg = $"# Error launching MarkItDown\n\n" +
                              $"**File:** `{sourcePath}`\n\n" +
                              $"**Reason:**\n```text\n{ex.Message}\n```";
            await File.WriteAllTextAsync(outboxPath, errorMsg);
        }
    }

    private string GetMarkItDownExecutablePath()
    {
        // default pipx path on macos/linux
        string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string pipxPath = Path.Combine(homeDir, ".local", "bin", "markitdown");

        if (File.Exists(pipxPath))
            return pipxPath;


        // fallback to system PATH if not found in default pipx directory
        return "markitdown";
    }
}