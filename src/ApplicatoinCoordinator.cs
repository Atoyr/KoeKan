using System;
using System.Windows;

using Medoz.KoeKan.Services;

namespace Medoz.KoeKan;

public class ApplicationCoordinator : IDisposable
{
    private readonly TrayManager _trayManager;

    public ApplicationCoordinator()
    {
        _trayManager = new TrayManager();

        Initialize();

        // イベントハンドラーを設定
        _trayManager.ShowMainWindowRequested += OnShowMainWindowRequested;
        _trayManager.ShowSettingsRequested += OnShowSettingsRequested;
        _trayManager.ExitRequested += OnExitRequested;
    }

    private void Initialize()
    {
        var ws = ServiceContainer.Instance.WindowService;
        ws.CreateMainWindow = (_) =>
        {
            return new MainWindow(new MainWindowViewModel());
        };
        ws.CreateSettingsWindow = (_) =>
        {
            SettingsWindow settingsWindow = new ();
            return settingsWindow;
        };
    }


    private void OnShowMainWindowRequested(object? sender, EventArgs e)
    {
        ToggleMainWindow();
    }

    private void OnShowSettingsRequested(object? sender, EventArgs e)
    {
        ShowSettingsWindow();
    }

    private void OnExitRequested(object? sender, EventArgs e)
    {
        System.Windows.Application.Current.Shutdown();
    }

    private void ToggleMainWindow()
    {
        ServiceContainer.Instance.WindowService.ShowMainWindow();
    }

    private void MainWindow_Closed(object? sender, EventArgs e)
    {
        // FIXME
    }

    private void ShowSettingsWindow()
    {
        ServiceContainer.Instance.WindowService.OpenSettingsWindow();
    }

    private void SettingsWindow_Closed(object? sender, EventArgs e)
    {
        ServiceContainer.Instance.WindowService.CloseSettingsWindow();
    }

    public void HandleMainWindowStateChanged(WindowState newState)
    {
    }

    public void Dispose()
    {
        // FIXME
        _trayManager?.Dispose();
    }
}