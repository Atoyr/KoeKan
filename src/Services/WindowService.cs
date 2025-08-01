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
    internal IConfigService ConfigService;

    public event EventHandler? OpenMainWindowRequested;
    public event EventHandler? CloseMainWindowRequested;

    public event EventHandler? WindowSizeChanged;
    public event EventHandler<bool>? MoveableWindowStateChanged;


    public event EventHandler? OpenSettingWindowRequested;
    public event EventHandler? CloseSettingWindowRequested;

    public WindowService(IConfigService configService)
    {
        ConfigService = configService;
    }

    private void InitializeMainWindow()
    {
        //
        MainWindow = App.GetInstance<MainWindow>();
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
        MainWindow!.Closed += (s, e) => CloseMainWindowRequested?.Invoke(s, e);
        OpenMainWindowRequested?.Invoke(MainWindow, EventArgs.Empty);
        MainWindow.ShowWindow();
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
        MainWindow!.ShowWindow();
    }

    public void HideMainWindow()
    {
        if (MainWindow == null)
        {
            throw new InvalidOperationException("MainWindow is not set.");
        }
        MainWindow.HideWindow();
    }

    private void SetMainWindowSizeWithConfig() =>
        SetMainWindowSize(
            ConfigService.GetConfig().Width,
            ConfigService.GetConfig().Height);


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
        var config = ConfigService.GetConfig();
        config.Width = MainWindow.Width;
        config.Height = MainWindow.Height;
        ConfigService.SaveConfig();
    }

    private void SetMainWindowPositionWithConfig()
    {
        var config = ConfigService.GetConfig();
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
        var config = ConfigService.GetConfig();
        config.X = MainWindow.Left;
        config.Y = MainWindow.Top;
        ConfigService.SaveConfig();
    }

    public bool IsMainWindowVisible()
    {
        if (MainWindow == null)
        {
            throw new InvalidOperationException("MainWindow is not set.");
        }
        return MainWindow.IsVisible;
    }

    public void MainWindowMessageClear()
    {
        MainWindow?.Clear();
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
        SettingsWindow = App.GetInstance<SettingsWindow>();
        if (SettingsWindow == null)
        {
            throw new InvalidOperationException("SettingsWindow is not set after invoking CreateSettingsWindow.");
        }
    }

    public void OpenSettingsWindow()
    {
        InitializeSettingsWindow();
        SettingsWindow!.Closed += (s, e) => CloseSettingWindowRequested?.Invoke(s,e);
        OpenSettingWindowRequested?.Invoke(SettingsWindow, EventArgs.Empty);
        SettingsWindow.ShowWindow();
    }

    public void CloseSettingsWindow()
    {
        SettingsWindow?.Close();
        SettingsWindow = null;
    }
}