
using System.Reactive.Disposables;

using Medoz.KoeKan.Clients;
using Medoz.KoeKan.Data;

namespace Medoz.KoeKan.Services;

public interface IWindowService
{
    Func<MainWindow>? CreateMainWindow { get; set; }
    Func<SettingsWindow>? CreateSettingsWindow { get; set; }

    void OpenMainWindow();
    void CloseMainWindow();
    void ShowMainWindow();
    void HideMainWindow();

    void SetMainWindowSize(double width, double height, bool save = true);
    void SetMainWindowPosition(double x, double y, bool save = true);

    void SaveMainWindowSize();
    void SaveMainWindowPosition();

    bool IsMainWindowVisible();

    void ToggleMoveableWindow();
    void ChangeMoveableWindowState(bool isMoveable);

    void SetMainWindowSize(double width, double height);
    void SetMainWindowPosition(double x, double y);

    bool OpenSettingWindow();

    event EventHandler? OpenMainWindowRequested;
    event EventHandler? CloseMainWindowRequested;
    event EventHandler? WindowSizeChanged;
    event EventHandler<bool>? MoveableWindowStateChanged;

    event EventHandler? OpenSettingWindowRequested;
    event EventHandler? CloseSettingWindowRequested;
}
