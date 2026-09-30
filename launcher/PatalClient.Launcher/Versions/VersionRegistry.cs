using PatalClient.Launcher.Models;

namespace PatalClient.Launcher.Versions;

public static class VersionRegistry
{
    private static readonly MinecraftVersion[] KnownVersions =
    [
        new()
        {
            VersionId = "1.21.x",
            DisplayName = "Minecraft 1.21.x",
            Supported = true,
            ClientComponent = "PatalClient-1.21.x.dll",
            Compatibility = "Requires Minecraft 1.21 or newer within the 1.21 series."
        }
    ];

    public static IReadOnlyList<MinecraftVersion> Versions => KnownVersions;

    public static MinecraftVersion? Find(string versionId) =>
        KnownVersions.FirstOrDefault(v => v.VersionId == versionId);

    public static MinecraftVersion Default => KnownVersions[0];
}
