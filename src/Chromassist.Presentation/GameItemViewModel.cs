using Chromassist.Core.Models;

namespace Chromassist.Presentation;

public sealed class GameItemViewModel(GameInstallation installation) : ObservableObject
{
    private GameValidationResult? _validation;

    public GameInstallation Installation { get; } = installation;

    public string DisplayName => $"{Installation.Title} — {Installation.RootDirectory}";

    public GameValidationResult? Validation
    {
        get => _validation;
        set
        {
            if (SetProperty(ref _validation, value))
            {
                OnPropertyChanged(nameof(StatusText));
                OnPropertyChanged(nameof(IsSupported));
            }
        }
    }

    public string StatusText => Validation?.Summary ?? "확인 대기";

    public bool IsSupported => Validation?.CanGeneratePatch == true;
}
