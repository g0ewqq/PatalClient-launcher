using System.IO;
using System.Text.Json;
using PatalClient.Launcher.Versions;

namespace PatalClient.Launcher.Models;

public sealed class LauncherConfiguration
{
    public const string LauncherVersion = "0.1.0";

    public string SelectedVersionId { get; set; } = VersionRegistry.Default.VersionId;
    public bool RememberWindowSize { get; set; }
    public bool CheckForUpdatesOnStart { get; set; }

    private static string GetConfigPath()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PatalClient");
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, "launcher.json");
    }

    public static LauncherConfiguration Load()
    {
        var path = GetConfigPath();
        try
        {
            if (!File.Exists(path))
                return new LauncherConfiguration();

            var config = JsonSerializer.Deserialize<LauncherConfiguration>(File.ReadAllText(path));
            return config ?? new LauncherConfiguration();
        }
        catch (Exception ex)
        {
            Logger.Warning($"Invalid configuration, resetting: {ex.Message}");
            return new LauncherConfiguration();
        }
    }

    public void Save()
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(GetConfigPath(), JsonSerializer.Serialize(this, options));
        }
        catch (Exception ex)
        {
            Logger.Error("Failed to save configuration", ex);
        }
    }
}
