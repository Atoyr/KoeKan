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
        ws.CreateMainWindow = () =>
        {
            return new MainWindow(new MainWindowViewModel());
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
        // FIXME
    }

    private void SettingsWindow_Closed(object? sender, EventArgs e)
    {
        // FIXME
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