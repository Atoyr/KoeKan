using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Interop;

using Medoz.CatChast.Auth;

namespace Medoz.KoeKan;

public class TrayManager : IDisposable
{
    private NotifyIcon? notifyIcon;

    // イベント定義
    public event EventHandler? ShowMainWindowRequested;
    public event EventHandler? ShowSettingsRequested;
    public event EventHandler? ExitRequested;

    public TrayManager()
    {
        InitializeNotifyIcon();
    }

    private void InitializeNotifyIcon()
    {
        notifyIcon = new NotifyIcon();

        try
        {
            var iconStream = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/app.ico"));
            if (iconStream != null)
            {
                notifyIcon.Icon = new Icon(iconStream.Stream);
            }
            else
            {
                notifyIcon.Icon = SystemIcons.Application;
            }
        }
        catch
        {
            notifyIcon.Icon = SystemIcons.Application;
        }

        UpdateNotifyIconText();
        notifyIcon.Visible = true;

        // イベントハンドラー設定
        notifyIcon.DoubleClick += OnNotifyIconDoubleClick;
        notifyIcon.MouseClick += OnNotifyIconMouseClick;

        CreateContextMenu();
    }

    private void UpdateNotifyIconText()
    {
        if (notifyIcon != null)
        {
            notifyIcon.Text = $"Your App Name (Shift)";
        }
    }

    private void CreateContextMenu()
    {
        var contextMenu = new ContextMenuStrip();

        // メイン画面表示
        var showMainMenuItem = new ToolStripMenuItem($"メイン画面を表示 ");
        showMainMenuItem.Click += (s, e) => ShowMainWindowRequested?.Invoke(this, EventArgs.Empty);
        showMainMenuItem.Font = new Font(showMainMenuItem.Font, FontStyle.Bold);
        contextMenu.Items.Add(showMainMenuItem);

        contextMenu.Items.Add(new ToolStripSeparator());

        // 設定
        var settingsMenuItem = new ToolStripMenuItem("設定");
        settingsMenuItem.Click += (s, e) => ShowSettingsRequested?.Invoke(this, EventArgs.Empty);
        contextMenu.Items.Add(settingsMenuItem);

        contextMenu.Items.Add(new ToolStripSeparator());

        // Auth
        var twitchOAuth = new ToolStripMenuItem("Twitch OAuth");
        twitchOAuth.Click += (s, e) => TwitchOAuth();
        contextMenu.Items.Add(twitchOAuth);

        contextMenu.Items.Add(new ToolStripSeparator());

        // 終了
        var exitMenuItem = new ToolStripMenuItem("終了");
        exitMenuItem.Click += (s, e) => ExitRequested?.Invoke(this, EventArgs.Empty);
        contextMenu.Items.Add(exitMenuItem);

        if (notifyIcon != null)
        {
            notifyIcon.ContextMenuStrip = contextMenu;
        }
    }

    private void OnNotifyIconDoubleClick(object? sender, EventArgs e)
    {
        ShowMainWindowRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OnNotifyIconMouseClick(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            ShowMainWindowRequested?.Invoke(this, EventArgs.Empty);
        }
    }

    public void ShowBalloonTip(string title, string text, ToolTipIcon icon = ToolTipIcon.Info)
    {
        if (notifyIcon != null)
        {
            notifyIcon.ShowBalloonTip(3000, title, text, icon);
        }
    }

    public void TwitchOAuth()
    {
    }

    // Refreshes the tray icon settings by updating the icon text and recreating the context menu.
    public void RefreshSettings()
    {
        UpdateNotifyIconText();
        CreateContextMenu();
    }

    public void Dispose()
    {
        if (notifyIcon != null)
        {
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
        }
    }
}