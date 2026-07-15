namespace Chromassist.Presentation;

public interface IExecutablePicker
{
    string? PickExecutable();
}

public interface IUserNotificationService
{
    void ShowInformation(string title, string message);
    void ShowError(string title, string message);
}
