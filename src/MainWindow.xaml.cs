using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Newtonsoft.Json.Bson;

using Medoz.KoeKan.Data;
using Medoz.KoeKan.Services;
using Medoz.KoeKan.Clients;

namespace Medoz.KoeKan;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{

    // 設定画面が開いているかどうか
    private bool isOpenModalWindow = false;

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetForegroundWindow(IntPtr hWnd);


    private HotKey? _hk;

    private readonly SemaphoreSlim _messageSemaphore = new(1, 1);

    private string? _activeProcessName;

    private Listener Listener { get; set; } = new ();

    public MainWindow()
    {
        InitializeComponent();

        // サービスの初期化
        var configService = new ConfigService();
        var listenerService = new ListenerService(Listener);
        var clientService = new ClientService(listenerService, configService);
        var windowService = new WindowService(this);

        SourceInitialized += ((sender, e) => {
            this.SetWindowTransparent(true);
        });

        windowService.MoveableWindowStateChanged += (s, e) => {
            if (e)
            {
                MoveWindowBar.Visibility = Visibility.Visible;
            }
            else
            {
                MoveWindowBar.Visibility = Visibility.Collapsed;
                ChatListBox.SelectedItem = null;
            }
        };

        // ViewModelの初期化
        DataContext = new MainWindowViewModel(
            configService,
            clientService,
            listenerService,
            windowService);
        MainWindowViewModel mwvm = (MainWindowViewModel)DataContext;
        // 初期ウィンドウのサイズを設定
        Width = mwvm.Width;
        Height = mwvm.Height;
        mwvm.Dispatcher = Dispatcher;

        // メッセージの変更を通知する
        Listener.Messages.CollectionChanged += (_, e) => {
            Dispatcher.BeginInvoke( new Action(() => { ChatListBox_ScrollToEnd(); }), System.Windows.Threading.DispatcherPriority.ContextIdle);
        };

        // 設定画面を開く
        mwvm.OpenSettingWindow = () => {
            SettingsWindow settingsWindow = new SettingsWindow();
            Topmost = false;
            settingsWindow.Owner = this;
            windowService.ChangeMoveableWindowState(false);
            isOpenModalWindow = true;
            bool? result = settingsWindow.ShowDialog();
            if (result == true)
            {
                // ユーザーがOKボタンをクリックした場合の処理
                // string setting1 = settingsWindow.Setting1;
                // 設定値を保存または反映する処理を追加
            }
            Topmost = true;
            isOpenModalWindow = false;
        };

        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        MainWindowViewModel mwvm = (MainWindowViewModel)DataContext;
        _hk = new HotKey(mwvm.ModKey, mwvm.Key, this);
        _hk.OnHotKeyPush += MessageBox_Focus;
        ChatListBox.ItemsSource = Listener.Messages;
        MoveWindowBar.Visibility = Visibility.Collapsed;
    }

    public void ActivateOtherWindow()
    {
        if (_activeProcessName is not null && ActivateOtherWindow(_activeProcessName))
        {
            return;
        }

        if (DataContext is not MainWindowViewModel mwvm)
        {
            return;
        }

        _activeProcessName = null;

        // 対象プロセスを順番に探索し、最初に見つかったプロセスをアクティブにする
        foreach(var pn in mwvm.Applications)
        {
            if (ActivateOtherWindow(pn))
            {
                _activeProcessName = pn;
                break;
            }
        }
        if (_activeProcessName is null)
        {
            // FIXME: メッセージボックスを表示する
        }
    }

    private bool ActivateOtherWindow(string processName)
    {
        var processes = Process.GetProcessesByName(processName);
        if (processes.Length > 0)
        {
            var process = processes[0]; // 最初のプロセスを使用
            IntPtr windowHandle = process.MainWindowHandle;
            SetForegroundWindow(windowHandle);
            return true;
        }
        return false;
    }

    public void MessageBox_Focus(object? sender, EventArgs e)
    {
        if (!isOpenModalWindow)
        {
            MessageBox.Focus();
            this.Activate();
        }
    }

    // メッセージボックスでキーが押されたときの処理
    public async void MessageBox_KeyDown(object sender, KeyEventArgs e)
    {
        // メッセージボックスでEnterキーが押された場合、メッセージを送信する
        if (e.Key == Key.Enter)
        {
            if (!await _messageSemaphore.WaitAsync(0))
            {
                return;
            }

            try
            {
                await MessageEnterAsync(MessageBox.Text);
            }
            finally
            {
                _messageSemaphore.Release();
            }
        }
        else if (e.Key == Key.Escape)
        {
            ActivateOtherWindow();
        }
    }

    /// <summary>
    /// メッセージボックスでEnterキーが押されたときの処理
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    private async Task MessageEnterAsync(string text)
    {
        if (!string.IsNullOrWhiteSpace(text)
            && text.Length > 0
            && DataContext is MainWindowViewModel mwvm)
        {
            if (text[0] == ':')
            {
                await mwvm.ExecuteCommand(text.Substring(1));
            }
            else
            {
                await mwvm.SendMessage(text);
            }
            MessageBox.Text = string.Empty;
        }
    }

    private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        => DragMove();

    private void ChatListBox_ScrollToEnd()
    {
        var item = ChatListBox.Items[ChatListBox.Items.Count - 1];
        ChatListBox.ScrollIntoView(item);
    }
}