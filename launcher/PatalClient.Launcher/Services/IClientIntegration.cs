using PatalClient.Launcher.Models;

namespace PatalClient.Launcher.Services;

public enum IntegrationFailure
{
    MinecraftNotDetected,
    UnsupportedVersion,
    NativeComponentMissing,
    IntegrationUnavailable,
    Unknown
}

public sealed class IntegrationResult
{
    public bool Succeeded { get; private init; }
    public IntegrationFailure Failure { get; private init; }
    public string? Detail { get; private init; }

    public static IntegrationResult Ok(string? detail = null) =>
        new() { Succeeded = true, Detail = detail };

    public static IntegrationResult Fail(IntegrationFailure failure, string? detail = null) =>
        new() { Succeeded = false, Failure = failure, Detail = detail };
}

public interface IClientIntegration
{
    LauncherState GetState();
    IntegrationResult DetectMinecraft();
    IntegrationResult ValidateVersion(MinecraftVersion version);
    IntegrationResult Initialize(MinecraftVersion version);
    IntegrationResult Inject(MinecraftVersion version);
    IntegrationResult Eject();
}
