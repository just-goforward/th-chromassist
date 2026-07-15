using System.Diagnostics;
using System.IO;
using System.Windows;
using Chromassist.Core.Services;
using Chromassist.Presentation;
using Microsoft.Win32;

namespace Chromassist.App;

public sealed class ExecutablePicker : IExecutablePicker
{
    public string? PickExecutable()
    {
        var dialog = new OpenFileDialog
        {
            Title = "th18.exe 선택",
            Filter = "Touhou 18 executable (th18.exe)|th18.exe",
            CheckFileExists = true,
            Multiselect = false
        };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}

public sealed class UserNotificationService : IUserNotificationService
{
    public void ShowInformation(string title, string message) =>
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);

    public void ShowError(string title, string message) =>
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
}

public sealed class ThcrapGameLauncher : IGameLauncher
{
    public Task LaunchAsync(
        string thcrapDirectory,
        string runConfigurationPath,
        string gameId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var executable = Path.Combine(thcrapDirectory, "thcrap.exe");
        if (!File.Exists(executable))
        {
            throw new FileNotFoundException("thcrap.exe를 찾을 수 없습니다.", executable);
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = executable,
            WorkingDirectory = thcrapDirectory,
            UseShellExecute = false
        };
        startInfo.ArgumentList.Add(Path.GetFileName(runConfigurationPath));
        startInfo.ArgumentList.Add(gameId);
        Process.Start(startInfo);
        return Task.CompletedTask;
    }
}
