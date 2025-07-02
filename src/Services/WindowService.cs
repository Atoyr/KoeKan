using System;
using System.Windows;
using System.Windows.Interop;
using System.Runtime.InteropServices;

using Medoz.KoeKan.Clients;
using Medoz.KoeKan.Data;
using Discord.Rest;

namespace Medoz.KoeKan.Services;

public class WindowService : IWindowService
{
    internal MainWindow? MainWindow;
    internal Window? SettingsWindow;

    public Func<MainWindow>? CreateMainWindow { get; set; }
    public Func<SettingsWindow>? CreateSettingsWindow { get; set; }

    public event EventHandler? OpenMainWindowRequested;
    public event EventHandler? CloseMainWindowRequested;

    public event EventHandler? WindowSizeChanged;
    public event EventHandler<bool>? MoveableWindowStateChanged;


    public event EventHandler? OpenSettingWindowRequested;
    public event EventHandler? CloseSettingWindowRequested;

    public WindowService() { }

    private void InitializeMainWindow()
    {
        MainWindow = CreateMainWindow?.Invoke();
        if (MainWindow == null)
        {
            throw new InvalidOperationException("MainWindow is not set after invoking CreateMainWindow.");
        }
    }

    public void OpenMainWindow()
    {
        InitializeMainWindow();
        MainWindow!.Show();
        MainWindow.ShowInTaskbar = true;
        MainWindow.Activate();
    }

    public void CloseMainWindow()
    {
        MainWindow?.Close();
        MainWindow = null;
    }

    public void SetMainWindow(MainWindow mainWindow)
    {
        MainWindow = mainWindow;
    }

    public void ShowMainWindow()
    {
        if (MainWindow == null)
        {
            InitializeMainWindow();
        }
        MainWindow!.Show();
        MainWindow.ShowInTaskbar = true;
        MainWindow.Activate();
    }

    public void HideMainWindow()
    {
        if (MainWindow == null)
        {
            throw new InvalidOperationException("MainWindow is not set.");
        }
        MainWindow.Hide();
        MainWindow.ShowInTaskbar = false;
    }

    public bool IsMainWindowVisible()
    {
        if (MainWindow == null)
        {
            throw new InvalidOperationException("MainWindow is not set.");
        }
        return MainWindow.IsVisible;
    }

    public bool OpenSettingWindow()
    {
        var settingsWindow = new SettingsWindow();
        settingsWindow.Owner = MainWindow;
        settingsWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ChangeMoveableWindowState(false);
        OpenSettingWindowRequested?.Invoke(this, EventArgs.Empty);
        var result = settingsWindow.ShowDialog() ?? false;

        ChangeMoveableWindowState(true);
        CloseMainWindowRequested?.Invoke(this, EventArgs.Empty);
        return result;
    }

    public void ToggleMoveableWindow()
    {
        if (MainWindow == null)
        {
            throw new InvalidOperationException("MainWindow is not set.");
        }
        // ウィンドウが透明でない場合は移動可能、透明な場合は移動不可とする
        var moveable = MainWindow.IsWindowTransparent();
        ChangeMoveableWindowState(moveable);
    }

    public void ChangeMoveableWindowState(bool moveable)
    {
        if (MainWindow == null)
        {
            throw new InvalidOperationException("MainWindow is not set.");
        }
        // 透明な状態は移動不可、透明でない状態は移動可能とする
        MainWindow.SetWindowTransparent(!moveable);
        MoveableWindowStateChanged?.Invoke(this, moveable);
    }

    public void SetMainWindowSize(double width, double height)
    {
        MainWindow?.SetWindowSize(width, height);
    }

    public void SetMainWindowPosition(double x, double y)
    {
        MainWindow?.SetWindowPosition(x, y);
    }
}