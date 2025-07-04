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
    internal SettingsWindow? SettingsWindow;

    public Func<IWindowService, MainWindow>? CreateMainWindow { get; set; }
    public Func<IWindowService, SettingsWindow>? CreateSettingsWindow { get; set; }

    public event EventHandler? OpenMainWindowRequested;
    public event EventHandler? CloseMainWindowRequested;

    public event EventHandler? WindowSizeChanged;
    public event EventHandler<bool>? MoveableWindowStateChanged;


    public event EventHandler? OpenSettingWindowRequested;
    public event EventHandler? CloseSettingWindowRequested;

    public WindowService() { }

    private void InitializeMainWindow()
    {
        MainWindow = CreateMainWindow?.Invoke(this);
        if (MainWindow == null)
        {
            throw new InvalidOperationException("MainWindow is not set after invoking CreateMainWindow.");
        }
        SetMainWindowSizeWithConfig();
        SetMainWindowPositionWithConfig();
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

    private void SetMainWindowSizeWithConfig() =>
        SetMainWindowSize(
            ServiceContainer.Instance.ConfigService.GetConfig().Width,
            ServiceContainer.Instance.ConfigService.GetConfig().Height);


    public void SetMainWindowSize(double width, double height, bool save = true)
    {
        if (MainWindow == null)
        {
            throw new InvalidOperationException("MainWindow is not set.");
        }
        MainWindow.Width = width;
        MainWindow.Height = height;

        if (save)
        {
            SaveMainWindowSize();
        }
        WindowSizeChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SaveMainWindowSize()
    {
        if (MainWindow == null)
        {
            throw new InvalidOperationException("MainWindow is not set.");
        }
        var config = ServiceContainer.Instance.ConfigService.GetConfig();
        config.Width = MainWindow.Width;
        config.Height = MainWindow.Height;
        ServiceContainer.Instance.ConfigService.SaveConfig();
    }

    private void SetMainWindowPositionWithConfig()
    {
        var config = ServiceContainer.Instance.ConfigService.GetConfig();
        SetMainWindowPosition(config.X, config.Y);
    }

    public void SetMainWindowPosition(double x, double y, bool save = true)
    {
        if (MainWindow == null)
        {
            throw new InvalidOperationException("MainWindow is not set.");
        }
        MainWindow.Left = x;
        MainWindow.Top = y;

        if (save)
        {
            SaveMainWindowPosition();
        }
        WindowSizeChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SaveMainWindowPosition()
    {
        if (MainWindow == null)
        {
            throw new InvalidOperationException("MainWindow is not set.");
        }
        var config = ServiceContainer.Instance.ConfigService.GetConfig();
        config.X = MainWindow.Left;
        config.Y = MainWindow.Top;
        ServiceContainer.Instance.ConfigService.SaveConfig();
    }

    public bool IsMainWindowVisible()
    {
        if (MainWindow == null)
        {
            throw new InvalidOperationException("MainWindow is not set.");
        }
        return MainWindow.IsVisible;
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
        if (!moveable)
        {
            // 移動不可にしたら保存する
            SaveMainWindowPosition();
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


    private void InitializeSettingsWindow()
    {
        SettingsWindow = CreateSettingsWindow?.Invoke(this);
        if (SettingsWindow == null)
        {
            throw new InvalidOperationException("SettingsWindow is not set after invoking CreateSettingsWindow.");
        }
    }

    public void OpenSettingsWindow()
    {
        InitializeSettingsWindow();
        SettingsWindow!.Show();
        SettingsWindow.ShowInTaskbar = true;
        SettingsWindow.Activate();
    }

    public void CloseSettingsWindow()
    {
        SettingsWindow?.Close();
        SettingsWindow = null;
    }
}