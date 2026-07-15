namespace Chromassist.Core.Models;

public sealed record GameInstallation(
    string GameId,
    string Title,
    string RootDirectory,
    string ExecutablePath,
    string DataArchivePath,
    string? ThcrapDirectory,
    InstallationSource Source)
{
    public string DisplayName => $"{Title} ({Source})";
}

public enum InstallationSource
{
    SteamManifest,
    KnownSteamPath,
    ThcrapConfiguration,
    Manual
}
