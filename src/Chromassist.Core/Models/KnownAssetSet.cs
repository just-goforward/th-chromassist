namespace Chromassist.Core.Models;

public sealed record KnownAssetSet(
    string Id,
    string GameId,
    string VersionLabel,
    string Distribution,
    string ExecutableSha256,
    string DataArchiveSha256,
    string MinimumThcrapVersion,
    IReadOnlyList<string> CandidateTextureNames);
