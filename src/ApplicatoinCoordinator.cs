using System;
using System.Windows;

using Medoz.KoeKan.Services;

namespace Medoz.KoeKan;

public class ApplicationCoordinator : IDisposable
{
    private readonly TrayManager _trayManager;
    private readonly IWindowService _windowService;

    public ApplicationCoordinator(IWindowService windowService)
    {
        _trayManager = new TrayManager();
        _windowService = windowService ?? throw new ArgumentNullException(nameof(windowService), "WindowService cannot be null.");

        // イベントハンドラーを設定
        _trayManager.ShowMainWindowRequested += OnShowMainWindowRequested;
        _trayManager.ShowSettingsRequested += OnShowSettingsRequested;
        _trayManager.ExitRequested += OnExitRequested;
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
        _windowService.ShowMainWindow();
    }

    private void MainWindow_Closed(object? sender, EventArgs e)
    {
        // FIXME
    }

    private void ShowSettingsWindow()
    {
        _windowService.OpenSettingsWindow();
    }

    private void SettingsWindow_Closed(object? sender, EventArgs e)
    {
        _windowService.CloseSettingsWindow();
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