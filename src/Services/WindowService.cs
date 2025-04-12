using System;
using System.Windows;
using System.Windows.Interop;
using System.Runtime.InteropServices;

using Medoz.KoeKan.Clients;
using Medoz.KoeKan.Data;

namespace Medoz.KoeKan.Services;

public class WindowService : IWindowService
{
    private readonly Window _window;

    public event EventHandler? WindowSizeChanged;
    public event EventHandler<bool>? MoveableWindowStateChanged;

    public WindowService(Window window)
    {
        _window = window ?? throw new ArgumentNullException(nameof(window));
    }

    public void Close()
    {
        _window.Close();
    }

    public void ToggleMoveableWindow()
    {
        // ウィンドウが透明でない場合は移動可能、透明な場合は移動不可とする
        var moveable = _window.IsWindowTransparent();
        ChangeMoveableWindowState(moveable);
    }

    public void ChangeMoveableWindowState(bool moveable)
    {
        // 透明な状態は移動不可、透明でない状態は移動可能とする
        _window.SetWindowTransparent(!moveable);
        MoveableWindowStateChanged?.Invoke(this, moveable);
    }

    public void SetWindowSize(double width, double height)
    {
        _window.SetWindowSize(width, height);
    }

    public void SetWindowPosition(double x, double y)
    {
        _window.SetWindowPosition(x, y);
    }
}