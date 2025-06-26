
using System.Reactive.Disposables;

using Medoz.KoeKan.Clients;
using Medoz.KoeKan.Data;

namespace Medoz.KoeKan.Services;

public interface IWindowService
{
    void Close();

    void ToggleMoveableWindow();
    void ChangeMoveableWindowState(bool isMoveable);

    void SetWindowSize(double width, double height);
    void SetWindowPosition(double x, double y);

    bool OpenSettingWindow();

    event EventHandler? WindowSizeChanged;
    event EventHandler<bool>? MoveableWindowStateChanged;

    event EventHandler? SettingWindowOpened;
    event EventHandler? SettingWindowClosed;
}
