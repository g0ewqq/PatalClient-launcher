using System.IO;
using PatalClient.Launcher.Models;

namespace PatalClient.Launcher.Services;

public sealed class NativeClientIntegration : IClientIntegration
{
    private LauncherState _state = LauncherState.Disconnected;

    public LauncherState GetState() => _state;

    public IntegrationResult DetectMinecraft()
    {
        var process = System.Diagnostics.Process.GetProcessesByName("javaw")
            .Concat(System.Diagnostics.Process.GetProcessesByName("java"))
            .FirstOrDefault();

        return process is null
            ? IntegrationResult.Fail(IntegrationFailure.MinecraftNotDetected, "No running Java process found.")
            : IntegrationResult.Ok($"Detected process {process.Id}.");
    }

    public IntegrationResult ValidateVersion(MinecraftVersion version)
    {
        if (!version.Supported)
            return IntegrationResult.Fail(IntegrationFailure.UnsupportedVersion,
                $"{version.DisplayName} is not supported by this build.");

        return IntegrationResult.Ok();
    }

    public IntegrationResult Initialize(MinecraftVersion version)
    {
        var component = LocateClientComponent(version);
        if (component is null)
            return IntegrationResult.Fail(IntegrationFailure.NativeComponentMissing,
                $"Client component '{version.ClientComponent ?? version.VersionId}' was not found.");

        _state = LauncherState.Ready;
        return IntegrationResult.Ok();
    }

    public IntegrationResult Inject(MinecraftVersion version)
    {
        return IntegrationResult.Fail(IntegrationFailure.IntegrationUnavailable,
            "Native injection is not implemented yet. The client integration arrives in a later phase.");
    }

    public IntegrationResult Eject()
    {
        return IntegrationResult.Fail(IntegrationFailure.IntegrationUnavailable,
            "Native ejection is not implemented yet.");
    }

    private static string? LocateClientComponent(MinecraftVersion version)
    {
        if (version.ClientComponent is null)
            return null;

        var candidate = Path.Combine(AppContext.BaseDirectory, "client", version.ClientComponent);
        return File.Exists(candidate) ? candidate : null;
    }
}
