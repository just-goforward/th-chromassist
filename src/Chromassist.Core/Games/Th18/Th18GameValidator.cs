using Chromassist.Core.Infrastructure;
using Chromassist.Core.Models;
using Chromassist.Core.Services;
using Chromassist.Core.Thcrap;

namespace Chromassist.Core.Games.Th18;

public sealed class Th18GameValidator(ThcrapInspector thcrapInspector) : IGameValidator
{
    public async Task<GameValidationResult> ValidateAsync(GameInstallation installation, CancellationToken cancellationToken = default)
    {
        var diagnostics = new List<string>();
        if (!File.Exists(installation.ExecutablePath) || !File.Exists(installation.DataArchivePath))
        {
            return new GameValidationResult(
                installation,
                ValidationStatus.MissingFiles,
                "th18.exe 또는 th18.dat가 없습니다.",
                null,
                null,
                null,
                ThcrapInspection.Missing,
                diagnostics);
        }

        try
        {
            var executableHashTask = FileHash.Sha256Async(installation.ExecutablePath, cancellationToken);
            var dataHashTask = FileHash.Sha256Async(installation.DataArchivePath, cancellationToken);
            await Task.WhenAll(executableHashTask, dataHashTask).ConfigureAwait(false);

            var executableHash = await executableHashTask.ConfigureAwait(false);
            var dataHash = await dataHashTask.ConfigureAwait(false);
            var assetSet = Th18AssetCatalog.KnownAssetSets.FirstOrDefault(set =>
                string.Equals(set.ExecutableSha256, executableHash, StringComparison.OrdinalIgnoreCase)
                && string.Equals(set.DataArchiveSha256, dataHash, StringComparison.OrdinalIgnoreCase));

            if (assetSet is null)
            {
                diagnostics.Add($"EXE SHA-256: {executableHash}");
                diagnostics.Add($"DAT SHA-256: {dataHash}");
                return new GameValidationResult(
                    installation,
                    ValidationStatus.UnknownVersion,
                    "지원 목록에 없는 TH18 설치본입니다. 안전을 위해 patch 생성을 중단합니다.",
                    null,
                    executableHash,
                    dataHash,
                    ThcrapInspection.Missing,
                    diagnostics);
            }

            var thcrap = thcrapInspector.Inspect(installation, assetSet.MinimumThcrapVersion);
            var status = thcrap.IsCompatible ? ValidationStatus.Supported : ValidationStatus.UnsupportedThcrap;
            var summary = thcrap.IsCompatible
                ? $"{assetSet.VersionLabel} {assetSet.Distribution} · {thcrap.Summary}"
                : thcrap.Summary;

            return new GameValidationResult(
                installation,
                status,
                summary,
                assetSet,
                executableHash,
                dataHash,
                thcrap,
                diagnostics);
        }
        catch (IOException exception)
        {
            diagnostics.Add(exception.Message);
            return new GameValidationResult(
                installation,
                ValidationStatus.Error,
                "게임 파일을 읽는 중 오류가 발생했습니다.",
                null,
                null,
                null,
                ThcrapInspection.Missing,
                diagnostics);
        }
    }
}
