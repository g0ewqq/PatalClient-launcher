using System.IO;

namespace PatalClient.Launcher;

public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error
}

public static class Logger
{
    private const int MaxFileSizeBytes = 1_000_000;
    private static readonly object Gate = new();
    private static string? _logPath;

    public static string LogPath
    {
        get
        {
            if (_logPath is null)
            {
                var dir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "PatalClient", "logs");
                Directory.CreateDirectory(dir);
                _logPath = Path.Combine(dir, "launcher.log");
            }
            return _logPath;
        }
    }

    public static void Debug(string message) => Write(LogLevel.Debug, message);
    public static void Info(string message) => Write(LogLevel.Info, message);
    public static void Warning(string message) => Write(LogLevel.Warning, message);

    public static void Error(string message, Exception? exception = null) =>
        Write(LogLevel.Error, exception is null ? message : $"{message}: {exception}");

    private static void Write(LogLevel level, string message)
    {
        try
        {
            lock (Gate)
            {
                var file = new FileInfo(LogPath);
                if (file.Exists && file.Length > MaxFileSizeBytes)
                    File.Delete(LogPath);

                File.AppendAllText(LogPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}{Environment.NewLine}");
            }
        }
        catch
        {
            // Logging must never break the launcher.
        }
    }
}
