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
        _window.ToggleWindowTransparent();
    }

    public void ChangeMoveableWindowState(bool isMoveable)
    {
        _window.SetWindowTransparent(!isMoveable);
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