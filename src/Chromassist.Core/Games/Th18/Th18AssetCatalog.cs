using Chromassist.Core.Models;

namespace Chromassist.Core.Games.Th18;

public static class Th18AssetCatalog
{
    public const string SteamAppId = "1566410";
    public const string GameDirectoryName = "th18";
    public const string ExecutableName = "th18.exe";
    public const string DataArchiveName = "th18.dat";

    public static IReadOnlyList<KnownAssetSet> KnownAssetSets { get; } =
    [
        new(
            "th18-v1.00a-steam-20260715",
            "th18",
            "v1.00a",
            "Steam original",
            "9ED66E6952459E81515C17A671410BEE7014A83E3C6CC6A7E360E7B4904C62F4",
            "3949E7C01BDEF9C3FE75711E088BFE4E195F3A657585C79B6A1AFB9D117DC800",
            "2025-12-02",
            ["bullet1.png", "bullet2.png", "bullet3.png", "bullet4.png", "bullet5.png", "bullet6.png"])
    ];
}
