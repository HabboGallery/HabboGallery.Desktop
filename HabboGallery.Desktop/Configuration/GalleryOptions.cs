using System.Text.Json.Serialization;

using HabboGallery.Desktop.Json;

namespace HabboGallery.Desktop.Configuration;

public record PlatformPaths
{
    public required HPlatform Platform { get; init; }

    public string RootPath { get; init; }
    public required string ClientPath { get; init; }
    public required string ExecutablePath { get; init; }
}

public sealed class GalleryOptions
{
    public string Email { get; set; }

    public required string[] UnityInterceptionTriggers { get; init; }
    public required string[] FlashInterceptionTriggers { get; init; }

    public required int GameListenPort { get; init; }
    public required int ProxyListenPort { get; init; }

    public required bool IsCheckingForUpdates { get; init; }

    public required string LauncherPath { get; set; }
    public required string[] ProxyOverrides { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public LauncherVersions Versions { get; internal set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public IReadOnlyDictionary<HPlatform, PlatformPaths>? PlatformPaths { get; internal set; }
}
