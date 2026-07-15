using Chromassist.Core.Models;

namespace Chromassist.Core.Presets;

public static class PresetCatalog
{
    public static IReadOnlyList<ColorPreset> All { get; } =
    [
        new("original", "Original", "원본 색상을 유지합니다.", PresetKind.Original, 0, 0, 1, "baseline", "0.1.0"),
        new("protan-standard", "Protan · 기본", "적색·녹색 역할군의 hue를 제한적으로 분리합니다.", PresetKind.Protan, 28, 26, 1.02, "experimental", "0.1.0"),
        new("protan-strong", "Protan · 강하게", "기본 preset보다 역할군 hue 분리를 크게 적용합니다.", PresetKind.Protan, 42, 38, 1.05, "experimental", "0.1.0"),
        new("deutan-standard", "Deutan · 기본", "녹색·적색 역할군의 hue를 제한적으로 분리합니다.", PresetKind.Deutan, -24, 31, 1.02, "experimental", "0.1.0"),
        new("deutan-strong", "Deutan · 강하게", "기본 preset보다 역할군 hue 분리를 크게 적용합니다.", PresetKind.Deutan, -36, 45, 1.05, "experimental", "0.1.0"),
        new("tritan-experimental", "Tritan · 실험적", "청색·황색 역할군을 위한 검증 전 preset입니다.", PresetKind.Tritan, -28, 24, 1.02, "experimental", "0.1.0")
    ];
}
