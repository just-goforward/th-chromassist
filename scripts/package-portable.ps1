param(
    [string]$OutputDirectory = "artifacts/portable",
    [string]$Runtime = "win-x64"
)

$ErrorActionPreference = "Stop"
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$output = [System.IO.Path]::GetFullPath((Join-Path $repositoryRoot $OutputDirectory))
$downloadRoot = Join-Path $repositoryRoot "work/package"
$archive = Join-Path $downloadRoot "thtk-win32-x64.zip"
$expanded = Join-Path $downloadRoot "expanded"
$expectedArchiveHash = "4B7C193434A52CA6FC418F243637B6971E6409AE1FB0F7C01F18E0645405EDFB"
$expectedTools = @{
    "thdat.exe" = "EF494069A048238948E4B8769A955935DAAD6061870A99B8CB230B64BD84AF9B"
    "thanm.exe" = "3182BE234BFBCC8480A883A52E84D7218F1AB7C28D5E75A2D98A4E498F6B9B8C"
}

function Assert-PathWithin([string]$Path, [string]$Root) {
    $resolvedPath = [System.IO.Path]::GetFullPath($Path)
    $resolvedRoot = [System.IO.Path]::GetFullPath($Root).TrimEnd([System.IO.Path]::DirectorySeparatorChar) + [System.IO.Path]::DirectorySeparatorChar
    if (-not $resolvedPath.StartsWith($resolvedRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing filesystem operation outside $resolvedRoot"
    }
}

New-Item -ItemType Directory -Force -Path $downloadRoot | Out-Null
Invoke-WebRequest -UseBasicParsing "https://thcrap.thpatch.net/thtk-nightly/thtk-win32-x64.zip" -OutFile $archive
$actualArchiveHash = (Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash
if ($actualArchiveHash -ne $expectedArchiveHash) {
    throw "THTK nightly hash changed. Expected $expectedArchiveHash, received $actualArchiveHash. Review upstream before updating the allowlist."
}

if (Test-Path -LiteralPath $expanded) {
    Assert-PathWithin $expanded $downloadRoot
    Remove-Item -LiteralPath $expanded -Recurse -Force
}
Expand-Archive -LiteralPath $archive -DestinationPath $expanded
$toolSource = Join-Path $expanded "thtk-bin-ci"
foreach ($entry in $expectedTools.GetEnumerator()) {
    $actual = (Get-FileHash -LiteralPath (Join-Path $toolSource $entry.Key) -Algorithm SHA256).Hash
    if ($actual -ne $entry.Value) {
        throw "$($entry.Key) hash mismatch."
    }
}

dotnet publish (Join-Path $repositoryRoot "src/Chromassist.App/Chromassist.App.csproj") `
    --configuration Release `
    --runtime $Runtime `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:DebugType=None `
    -o $output

$toolDestination = Join-Path $output "tools/thtk"
New-Item -ItemType Directory -Force -Path $toolDestination | Out-Null
Copy-Item -LiteralPath (Join-Path $toolSource "thdat.exe") -Destination $toolDestination
Copy-Item -LiteralPath (Join-Path $toolSource "thanm.exe") -Destination $toolDestination
Copy-Item -LiteralPath (Join-Path $toolSource "thtk.dll") -Destination $toolDestination
Copy-Item -LiteralPath (Join-Path $toolSource "vcomp140.dll") -Destination $toolDestination
Copy-Item -LiteralPath (Join-Path $toolSource "COPYING.txt") -Destination $toolDestination
Copy-Item -LiteralPath (Join-Path $toolSource "COPYING.libpng.txt") -Destination $toolDestination
Copy-Item -LiteralPath (Join-Path $toolSource "COPYING.zlib.txt") -Destination $toolDestination
Copy-Item -LiteralPath (Join-Path $repositoryRoot "LICENSE") -Destination $output
Copy-Item -LiteralPath (Join-Path $repositoryRoot "THIRD_PARTY_NOTICES.md") -Destination $output

$zip = "$output.zip"
if (Test-Path -LiteralPath $zip) {
    Remove-Item -LiteralPath $zip -Force
}
Compress-Archive -Path (Join-Path $output "*") -DestinationPath $zip
Write-Host "Portable package: $zip"
