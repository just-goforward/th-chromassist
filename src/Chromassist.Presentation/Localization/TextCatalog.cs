using System.Globalization;

namespace Chromassist.Presentation.Localization;

public sealed class TextCatalog
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> Values =
        new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["ko"] = new Dictionary<string, string>
            {
                ["AppTitle"] = "Touhou Chromassist",
                ["Subtitle"] = "동방홍룡동 탄막 색상 접근성 도구",
                ["GameSection"] = "1. 게임",
                ["PresetSection"] = "2. 색각 프리셋",
                ["PreviewSection"] = "미리보기",
                ["Rescan"] = "다시 찾기",
                ["Browse"] = "실행 파일 직접 지정",
                ["ApplyLaunch"] = "적용하고 게임 실행",
                ["Original"] = "원본",
                ["Adjusted"] = "변경 후",
                ["Ready"] = "게임을 찾고 있습니다…",
                ["NoGame"] = "지원되는 TH18 설치본을 찾지 못했습니다. 실행 파일을 직접 지정하세요.",
                ["Experimental"] = "프리셋은 실험 단계이며 의학적 진단이나 검증된 공정성 기준이 아닙니다.",
                ["Complete"] = "패치를 생성하고 thcrap으로 게임을 실행했습니다.",
                ["ErrorTitle"] = "Touhou Chromassist 오류",
                ["InfoTitle"] = "Touhou Chromassist"
            },
            ["ja"] = new Dictionary<string, string>
            {
                ["AppTitle"] = "Touhou Chromassist",
                ["Subtitle"] = "東方虹龍洞 弾幕色アクセシビリティツール",
                ["GameSection"] = "1. ゲーム",
                ["PresetSection"] = "2. 色覚プリセット",
                ["PreviewSection"] = "プレビュー",
                ["Rescan"] = "再検索",
                ["Browse"] = "実行ファイルを指定",
                ["ApplyLaunch"] = "適用してゲームを起動",
                ["Original"] = "元画像",
                ["Adjusted"] = "変更後",
                ["Ready"] = "ゲームを検索しています…",
                ["NoGame"] = "対応するTH18を検出できませんでした。実行ファイルを指定してください。",
                ["Experimental"] = "プリセットは実験段階であり、医学的診断や検証済みの公平性基準ではありません。",
                ["Complete"] = "パッチを生成し、thcrapでゲームを起動しました。",
                ["ErrorTitle"] = "Touhou Chromassist エラー",
                ["InfoTitle"] = "Touhou Chromassist"
            },
            ["en"] = new Dictionary<string, string>
            {
                ["AppTitle"] = "Touhou Chromassist",
                ["Subtitle"] = "Touhou 18 bullet-colour accessibility tool",
                ["GameSection"] = "1. Game",
                ["PresetSection"] = "2. Colour-vision preset",
                ["PreviewSection"] = "Preview",
                ["Rescan"] = "Scan again",
                ["Browse"] = "Choose executable",
                ["ApplyLaunch"] = "Apply and launch game",
                ["Original"] = "Original",
                ["Adjusted"] = "Adjusted",
                ["Ready"] = "Looking for the game…",
                ["NoGame"] = "No supported TH18 installation was found. Choose the executable manually.",
                ["Experimental"] = "Presets are experimental; they are not medical diagnoses or validated fairness standards.",
                ["Complete"] = "The patch was generated and the game was launched through thcrap.",
                ["ErrorTitle"] = "Touhou Chromassist error",
                ["InfoTitle"] = "Touhou Chromassist"
            }
        };

    private string _language = NormalizeLanguage(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);

    public string Language
    {
        get => _language;
        set => _language = NormalizeLanguage(value);
    }

    public string this[string key] => Values[_language].TryGetValue(key, out var value) ? value : key;

    private static string NormalizeLanguage(string language) => Values.ContainsKey(language) ? language : "en";
}
