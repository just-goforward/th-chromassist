namespace Chromassist.Core.Models;

public sealed record GameValidationResult(
    GameInstallation Installation,
    ValidationStatus Status,
    string Summary,
    KnownAssetSet? AssetSet,
    string? ExecutableSha256,
    string? DataArchiveSha256,
    ThcrapInspection Thcrap,
    IReadOnlyList<string> Diagnostics)
{
    public bool CanGeneratePatch => Status == ValidationStatus.Supported && AssetSet is not null && Thcrap.IsCompatible;
}

public enum ValidationStatus
{
    Supported,
    MissingFiles,
    UnknownVersion,
    UnsupportedThcrap,
    InvalidPatchStack,
    Error
}

public sealed record ThcrapInspection(
    bool IsInstalled,
    bool IsCompatible,
    string? Version,
    string? RunConfigurationPath,
    IReadOnlyList<string> PatchArchives,
    string Summary)
{
    public static ThcrapInspection Missing { get; } = new(false, false, null, null, [], "thcrap을 찾을 수 없습니다.");
}
