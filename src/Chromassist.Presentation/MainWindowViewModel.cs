using System.Collections.ObjectModel;
using Chromassist.Core.Imaging;
using Chromassist.Core.Models;
using Chromassist.Core.Presets;
using Chromassist.Core.Services;
using Chromassist.Presentation.Localization;

namespace Chromassist.Presentation;

public sealed class MainWindowViewModel : ObservableObject, IAsyncDisposable
{
    private readonly IGameLocator _gameLocator;
    private readonly IGameValidator _gameValidator;
    private readonly IResourceExtractor _resourceExtractor;
    private readonly IPatchBuilder _patchBuilder;
    private readonly IGameLauncher _gameLauncher;
    private readonly IExecutablePicker _executablePicker;
    private readonly IUserNotificationService _notifications;
    private readonly TextCatalog _texts;
    private GameItemViewModel? _selectedGame;
    private ColorPreset _selectedPreset = PresetCatalog.All[1];
    private ExtractionResult? _extraction;
    private string? _preparedExecutable;
    private byte[]? _originalPreview;
    private byte[]? _adjustedPreview;
    private string _statusText;
    private bool _isBusy;

    public MainWindowViewModel(
        IGameLocator gameLocator,
        IGameValidator gameValidator,
        IResourceExtractor resourceExtractor,
        IPatchBuilder patchBuilder,
        IGameLauncher gameLauncher,
        IExecutablePicker executablePicker,
        IUserNotificationService notifications,
        TextCatalog? texts = null)
    {
        _gameLocator = gameLocator;
        _gameValidator = gameValidator;
        _resourceExtractor = resourceExtractor;
        _patchBuilder = patchBuilder;
        _gameLauncher = gameLauncher;
        _executablePicker = executablePicker;
        _notifications = notifications;
        _texts = texts ?? new TextCatalog();
        _statusText = _texts["Ready"];

        ScanCommand = new AsyncRelayCommand(ScanAsync, () => !IsBusy);
        BrowseCommand = new AsyncRelayCommand(BrowseAsync, () => !IsBusy);
        ApplyAndLaunchCommand = new AsyncRelayCommand(ApplyAndLaunchAsync, CanApply);
        ChangeLanguageCommand = new RelayCommand(NotifyLocalizedProperties);
    }

    public ObservableCollection<GameItemViewModel> Games { get; } = [];

    public IReadOnlyList<ColorPreset> Presets { get; } = PresetCatalog.All.Where(static preset => !preset.IsOriginal).ToArray();

    public IReadOnlyList<string> Languages { get; } = ["ko", "ja", "en"];

    public AsyncRelayCommand ScanCommand { get; }

    public AsyncRelayCommand BrowseCommand { get; }

    public AsyncRelayCommand ApplyAndLaunchCommand { get; }

    public RelayCommand ChangeLanguageCommand { get; }

    public GameItemViewModel? SelectedGame
    {
        get => _selectedGame;
        set
        {
            if (SetProperty(ref _selectedGame, value))
            {
                ApplyAndLaunchCommand.NotifyCanExecuteChanged();
                if (!IsBusy)
                {
                    _ = PrepareChangedSelectionAsync();
                }
            }
        }
    }

    public ColorPreset SelectedPreset
    {
        get => _selectedPreset;
        set
        {
            if (SetProperty(ref _selectedPreset, value))
            {
                UpdatePreview();
            }
        }
    }

    public string SelectedLanguage
    {
        get => _texts.Language;
        set
        {
            if (!string.Equals(_texts.Language, value, StringComparison.OrdinalIgnoreCase))
            {
                _texts.Language = value;
                OnPropertyChanged();
                NotifyLocalizedProperties();
            }
        }
    }

    public byte[]? OriginalPreview
    {
        get => _originalPreview;
        private set => SetProperty(ref _originalPreview, value);
    }

    public byte[]? AdjustedPreview
    {
        get => _adjustedPreview;
        private set => SetProperty(ref _adjustedPreview, value);
    }

    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                ScanCommand.NotifyCanExecuteChanged();
                BrowseCommand.NotifyCanExecuteChanged();
                ApplyAndLaunchCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public string AppTitle => _texts["AppTitle"];
    public string Subtitle => _texts["Subtitle"];
    public string GameSection => _texts["GameSection"];
    public string PresetSection => _texts["PresetSection"];
    public string PreviewSection => _texts["PreviewSection"];
    public string RescanLabel => _texts["Rescan"];
    public string BrowseLabel => _texts["Browse"];
    public string ApplyLaunchLabel => _texts["ApplyLaunch"];
    public string OriginalLabel => _texts["Original"];
    public string AdjustedLabel => _texts["Adjusted"];
    public string ExperimentalNotice => _texts["Experimental"];

    public Task InitializeAsync() => ScanAsync();

    public async ValueTask DisposeAsync()
    {
        if (_extraction is not null)
        {
            await _extraction.DisposeAsync().ConfigureAwait(false);
        }
    }

    private async Task ScanAsync()
    {
        await ExecuteBusyAsync(async () =>
        {
            Games.Clear();
            await ResetExtractionAsync().ConfigureAwait(true);
            var installations = await _gameLocator.FindInstalledGamesAsync().ConfigureAwait(true);
            foreach (var installation in installations)
            {
                var item = new GameItemViewModel(installation);
                Games.Add(item);
                item.Validation = await _gameValidator.ValidateAsync(installation).ConfigureAwait(true);
            }

            SelectedGame = Games.FirstOrDefault(static game => game.IsSupported) ?? Games.FirstOrDefault();
            if (SelectedGame is null)
            {
                StatusText = _texts["NoGame"];
                return;
            }

            StatusText = SelectedGame.StatusText;
            await PreparePreviewAsync().ConfigureAwait(true);
        }).ConfigureAwait(true);
    }

    private async Task BrowseAsync()
    {
        var executable = _executablePicker.PickExecutable();
        if (string.IsNullOrWhiteSpace(executable))
        {
            return;
        }

        await ExecuteBusyAsync(async () =>
        {
            var installation = _gameLocator.FromExecutable(executable);
            if (installation is null)
            {
                StatusText = _texts["NoGame"];
                return;
            }

            var item = Games.FirstOrDefault(game =>
                string.Equals(game.Installation.ExecutablePath, installation.ExecutablePath, StringComparison.OrdinalIgnoreCase));
            if (item is null)
            {
                item = new GameItemViewModel(installation);
                Games.Add(item);
            }

            item.Validation = await _gameValidator.ValidateAsync(installation).ConfigureAwait(true);
            SelectedGame = item;
            StatusText = item.StatusText;
            await ResetExtractionAsync().ConfigureAwait(true);
            await PreparePreviewAsync().ConfigureAwait(true);
        }).ConfigureAwait(true);
    }

    private async Task PreparePreviewAsync()
    {
        if (SelectedGame?.Validation?.CanGeneratePatch != true)
        {
            ApplyAndLaunchCommand.NotifyCanExecuteChanged();
            return;
        }

        _extraction = await _resourceExtractor.ExtractBulletTexturesAsync(SelectedGame.Validation).ConfigureAwait(true);
        if (!_extraction.Success)
        {
            StatusText = string.Join(Environment.NewLine, _extraction.Diagnostics);
            return;
        }

        UpdatePreview();
        _preparedExecutable = SelectedGame.Installation.ExecutablePath;
        StatusText = $"{SelectedGame.StatusText} · {_extraction.Textures.Count}개 texture 준비됨";
        ApplyAndLaunchCommand.NotifyCanExecuteChanged();
    }

    private void UpdatePreview()
    {
        var previewPath = _extraction?.Textures.FirstOrDefault()?.FilePath;
        if (previewPath is null || !File.Exists(previewPath))
        {
            OriginalPreview = null;
            AdjustedPreview = null;
            return;
        }

        var source = PngCodec.Read(previewPath);
        var adjusted = PresetTransformer.Transform(source, SelectedPreset);
        OriginalPreview = Encode(source);
        AdjustedPreview = Encode(adjusted);
    }

    private async Task ApplyAndLaunchAsync()
    {
        await ExecuteBusyAsync(async () =>
        {
            var validation = SelectedGame?.Validation;
            if (validation is null || _extraction is null)
            {
                return;
            }

            var result = await _patchBuilder.BuildAsync(validation, _extraction, SelectedPreset).ConfigureAwait(true);
            if (!result.Success || result.RunConfigurationPath is null || validation.Installation.ThcrapDirectory is null)
            {
                var message = string.Join(Environment.NewLine, result.Diagnostics.Prepend(result.Summary));
                StatusText = message;
                _notifications.ShowError(_texts["ErrorTitle"], message);
                return;
            }

            await _gameLauncher.LaunchAsync(
                validation.Installation.ThcrapDirectory,
                result.RunConfigurationPath,
                validation.Installation.GameId).ConfigureAwait(true);
            StatusText = _texts["Complete"];
        }).ConfigureAwait(true);
    }

    private async Task ExecuteBusyAsync(Func<Task> action)
    {
        IsBusy = true;
        try
        {
            await action().ConfigureAwait(true);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            StatusText = exception.Message;
            _notifications.ShowError(_texts["ErrorTitle"], exception.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ResetExtractionAsync()
    {
        if (_extraction is not null)
        {
            await _extraction.DisposeAsync().ConfigureAwait(true);
            _extraction = null;
        }

        _preparedExecutable = null;

        OriginalPreview = null;
        AdjustedPreview = null;
        ApplyAndLaunchCommand.NotifyCanExecuteChanged();
    }

    private bool CanApply() =>
        !IsBusy &&
        SelectedGame?.Validation?.CanGeneratePatch == true &&
        _extraction?.Success == true &&
        string.Equals(_preparedExecutable, SelectedGame.Installation.ExecutablePath, StringComparison.OrdinalIgnoreCase);

    private async Task PrepareChangedSelectionAsync()
    {
        await ExecuteBusyAsync(async () =>
        {
            await ResetExtractionAsync().ConfigureAwait(true);
            if (SelectedGame is not null)
            {
                StatusText = SelectedGame.StatusText;
                await PreparePreviewAsync().ConfigureAwait(true);
            }
        }).ConfigureAwait(true);
    }

    private static byte[] Encode(RgbaImage image)
    {
        using var stream = new MemoryStream();
        PngCodec.Write(stream, image);
        return stream.ToArray();
    }

    private void NotifyLocalizedProperties()
    {
        foreach (var property in new[]
        {
            nameof(AppTitle), nameof(Subtitle), nameof(GameSection), nameof(PresetSection), nameof(PreviewSection),
            nameof(RescanLabel), nameof(BrowseLabel), nameof(ApplyLaunchLabel), nameof(OriginalLabel), nameof(AdjustedLabel),
            nameof(ExperimentalNotice)
        })
        {
            OnPropertyChanged(property);
        }
    }
}
