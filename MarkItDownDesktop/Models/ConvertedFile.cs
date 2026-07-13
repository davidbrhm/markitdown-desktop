using System.IO;

namespace MarkItDownDesktop.Models;

public record ConvertedFile(
    string Name,
    string Path,
    string SizeText,
    string TimeText
)
{
    public static ConvertedFile FromPath(string filePath, string? customTime = null)
    {
        FileInfo fileInfo = new(filePath);

        string name = fileInfo.Name;
        string sizeText = FormatSize(fileInfo.Length);
        string timeText = customTime ?? fileInfo.LastWriteTime.ToString("HH:mm:ss");

        return new ConvertedFile(name, filePath, sizeText, timeText);
    }

    private static string FormatSize(long bytes)
    {
        double kb = bytes / 1024.0;
        if (kb < 1) return $"{bytes} B";

        double mb = kb / 1024.0;
        if (mb < 1) return $"{kb:F1} KB";

        return $"{mb:F1} MB";
    }
}