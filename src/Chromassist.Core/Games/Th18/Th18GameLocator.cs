using System.Text.RegularExpressions;
using Chromassist.Core.Models;
using Chromassist.Core.Services;

namespace Chromassist.Core.Games.Th18;

public sealed partial class Th18GameLocator : IGameLocator
{
    public Task<IReadOnlyList<GameInstallation>> FindInstalledGamesAsync(CancellationToken cancellationToken = default)
    {
        return Task.Run<IReadOnlyList<GameInstallation>>(() =>
        {
            var candidates = new Dictionary<string, InstallationSource>(StringComparer.OrdinalIgnoreCase);

            foreach (var steamRoot in FindSteamRoots())
            {
                cancellationToken.ThrowIfCancellationRequested();
                AddFromSteamRoot(steamRoot, candidates);
            }

            foreach (var drive in DriveInfo.GetDrives().Where(static drive => drive.IsReady && drive.DriveType == DriveType.Fixed))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var knownPath = Path.Combine(drive.RootDirectory.FullName, "Program Files (x86)", "Steam", "steamapps", "common", Th18AssetCatalog.GameDirectoryName);
                if (File.Exists(Path.Combine(knownPath, Th18AssetCatalog.ExecutableName)))
                {
                    candidates.TryAdd(Path.GetFullPath(knownPath), InstallationSource.KnownSteamPath);
                }
            }

            return candidates
                .Select(static pair => CreateInstallation(pair.Key, pair.Value))
                .Where(static installation => installation is not null)
                .Cast<GameInstallation>()
                .OrderBy(static installation => installation.RootDirectory, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }, cancellationToken);
    }

    public GameInstallation? FromExecutable(string executablePath)
    {
        if (string.IsNullOrWhiteSpace(executablePath))
        {
            return null;
        }

        var fullPath = Path.GetFullPath(executablePath);
        if (!File.Exists(fullPath) || !string.Equals(Path.GetFileName(fullPath), Th18AssetCatalog.ExecutableName, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return CreateInstallation(Path.GetDirectoryName(fullPath)!, InstallationSource.Manual);
    }

    private static IEnumerable<string> FindSteamRoots()
    {
        var roots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        if (!string.IsNullOrWhiteSpace(programFilesX86))
        {
            roots.Add(Path.Combine(programFilesX86, "Steam"));
        }

        foreach (var drive in DriveInfo.GetDrives().Where(static drive => drive.IsReady && drive.DriveType == DriveType.Fixed))
        {
            roots.Add(Path.Combine(drive.RootDirectory.FullName, "Steam"));
            roots.Add(Path.Combine(drive.RootDirectory.FullName, "Program Files", "Steam"));
            roots.Add(Path.Combine(drive.RootDirectory.FullName, "Program Files (x86)", "Steam"));
        }

        return roots.Where(Directory.Exists);
    }

    private static void AddFromSteamRoot(string steamRoot, IDictionary<string, InstallationSource> candidates)
    {
        var libraries = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { steamRoot };
        var libraryFile = Path.Combine(steamRoot, "steamapps", "libraryfolders.vdf");
        if (File.Exists(libraryFile))
        {
            foreach (Match match in SteamPathRegex().Matches(File.ReadAllText(libraryFile)))
            {
                var path = match.Groups[1].Value.Replace("\\\\", "\\", StringComparison.Ordinal);
                if (Directory.Exists(path))
                {
                    libraries.Add(path);
                }
            }
        }

        foreach (var library in libraries)
        {
            var manifest = Path.Combine(library, "steamapps", $"appmanifest_{Th18AssetCatalog.SteamAppId}.acf");
            if (!File.Exists(manifest))
            {
                continue;
            }

            var content = File.ReadAllText(manifest);
            var installDir = InstallDirRegex().Match(content);
            var directoryName = installDir.Success ? installDir.Groups[1].Value : Th18AssetCatalog.GameDirectoryName;
            var gameRoot = Path.Combine(library, "steamapps", "common", directoryName);
            if (File.Exists(Path.Combine(gameRoot, Th18AssetCatalog.ExecutableName)))
            {
                candidates[Path.GetFullPath(gameRoot)] = InstallationSource.SteamManifest;
            }
        }
    }

    private static GameInstallation? CreateInstallation(string rootDirectory, InstallationSource source)
    {
        var root = Path.GetFullPath(rootDirectory);
        var executable = Path.Combine(root, Th18AssetCatalog.ExecutableName);
        var data = Path.Combine(root, Th18AssetCatalog.DataArchiveName);
        if (!File.Exists(executable))
        {
            return null;
        }

        var thcrap = Path.Combine(root, "thcrap");
        return new GameInstallation(
            "th18",
            "동방홍룡동 ~ Unconnected Marketeers",
            root,
            executable,
            data,
            Directory.Exists(thcrap) ? thcrap : null,
            source);
    }

    [GeneratedRegex("\\\"path\\\"\\s+\\\"([^\\\"]+)\\\"", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex SteamPathRegex();

    [GeneratedRegex("\\\"installdir\\\"\\s+\\\"([^\\\"]+)\\\"", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex InstallDirRegex();
}
