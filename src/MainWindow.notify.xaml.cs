namespace Medoz.KoeKan;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private NotifyIcon? _notifyIcon;
    private void InitializeNotifyIcon()
    {
        _notifyIcon = new NotifyIcon();

        // .NET 8では、アイコンリソースの取得方法が少し異なります
        try
        {
            // アプリケーションのアイコンを使用（存在する場合）
            var iconStream = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/app.ico"));
            if (iconStream != null)
            {
                _notifyIcon.Icon = new Icon(iconStream.Stream);
            }
            else
            {
                // デフォルトのアプリケーションアイコンを使用
                _notifyIcon.Icon = SystemIcons.Application;
            }
        }
        catch
        {
            // アイコンの読み込みに失敗した場合はデフォルトアイコンを使用
            _notifyIcon.Icon = SystemIcons.Application;
        }

        _notifyIcon.Text = "Your App Name (Ctrl+Shift+Space)";
        _notifyIcon.Visible = true;

        // イベントハンドラーの設定
        // _notifyIcon.DoubleClick += NotifyIcon_DoubleClick;
        // _notifyIcon.MouseClick += NotifyIcon_MouseClick;

        CreateContextMenu();
    }

    private void CreateContextMenu()
    {
        var contextMenu = new ContextMenuStrip();

        // 表示メニュー
        var showMenuItem = new ToolStripMenuItem("表示 (Ctrl+Shift+Space)");
        // showMenuItem.Click += ShowMenuItem_Click;
        // showMenuItem.Font = new Font(showMenuItem.Font, FontStyle.Bold);
        contextMenu.Items.Add(showMenuItem);

        // 区切り線
        contextMenu.Items.Add(new ToolStripSeparator());

        // 設定メニュー
        var settingsMenuItem = new ToolStripMenuItem("設定");
        // settingsMenuItem.Click += SettingsMenuItem_Click;
        contextMenu.Items.Add(settingsMenuItem);

        // 区切り線
        contextMenu.Items.Add(new ToolStripSeparator());

        // Windows起動時に開始
        var startupMenuItem = new ToolStripMenuItem("Windows起動時に開始");
        startupMenuItem.CheckOnClick = true;
        // startupMenuItem.Click += StartupMenuItem_Click;
        contextMenu.Items.Add(startupMenuItem);

        // 区切り線
        contextMenu.Items.Add(new ToolStripSeparator());

        // 終了メニュー
        var exitMenuItem = new ToolStripMenuItem("終了");
        // exitMenuItem.Click += ExitMenuItem_Click;
        contextMenu.Items.Add(exitMenuItem);

        if (_notifyIcon != null)
        {
            _notifyIcon.ContextMenuStrip = contextMenu;
        }
    }
}