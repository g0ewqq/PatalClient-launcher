namespace PatalClient.Launcher.Models;

public sealed class MinecraftVersion
{
    public required string VersionId { get; init; }
    public required string DisplayName { get; init; }
    public bool Supported { get; init; }
    public string? ClientComponent { get; init; }
    public string? Compatibility { get; init; }

    public override string ToString() => DisplayName;
}
