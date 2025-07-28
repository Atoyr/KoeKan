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
using System.Net.WebSockets;
using System.Drawing.Imaging;
using Microsoft.Extensions.Logging;

namespace Medoz.KoeKan;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetForegroundWindow(IntPtr hWnd);


    private HotKey? _hk;

    private readonly SemaphoreSlim _messageSemaphore = new(1, 1);

    private string? _activeProcessName;

    private readonly Listener _listener = new();

    private MainWindow() { }

    public MainWindow(MainWindowViewModel mwvm) : this()
    {
        DataContext = mwvm;
        // 初期ウィンドウのサイズを設定
        Width = mwvm.Width;
        Height = mwvm.Height;

        InitializeComponent();
        SourceInitialized += ((_, _) =>
        {
            this.SetWindowTransparent(true);
        });

        mwvm.WindowService.MoveableWindowStateChanged += (s, e) =>
        {
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

        _listener.Dispatcher = Dispatcher;
        // メッセージの変更を通知する
        _listener.Messages.CollectionChanged += (_, e) =>
        {
            Dispatcher.BeginInvoke(new Action(() => { ChatListBox_ScrollToEnd(); }), System.Windows.Threading.DispatcherPriority.ContextIdle);
        };

        // メッセージの受信を購読
        mwvm.AsyncEventBus.Subscribe<ClientMessage>((message) =>
        {
            try
            {
                var chatMessageType = ChatMessageTypeExtension.FromString(message.ClientType);
                var chatMessage = new ChatMessage(
                    chatMessageType,
                    message.Channel,
                    message.IconSource,
                    message.Username,
                    message.Content,
                    message.Timestamp.Date);
                _listener.AddMessage(chatMessage);
            }
            catch (Exception ex)
            {
                // メッセージの変換に失敗した場合はログに出力
                mwvm.Logger.LogError(ex, "Failed to convert message type.");
            }
        });
        // serverService.WebApiMessageReceived += async (s, e) =>
        // {
        //     // WebAPIからのメッセージを受信したときの処理
        //     // FIXME: 通常のテキスト入力と同じ形になっている
        //     await MessageEnterAsync(e);
        // };

        // serverService.StartWebApiAsync();

        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        MainWindowViewModel mwvm = (MainWindowViewModel)DataContext;
        _hk = new HotKey(mwvm.ModKey, mwvm.Key, this);
        _hk.OnHotKeyPush += MessageBox_Focus;
        ChatListBox.ItemsSource = _listener.Messages;
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
        foreach (var pn in mwvm.Applications)
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
        MessageBox.Focus();
        this.Activate();
    }

    /// <summary>
    /// メッセージボックスでキーが押されたときの処理のハンドラ
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public async void MessageBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
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
        if (ChatListBox.Items.Count == 0)
        {
            return;
        }
        var item = ChatListBox.Items[ChatListBox.Items.Count - 1];
        ChatListBox.ScrollIntoView(item);
    }

    public void Clear()
    {
        if (DataContext is MainWindowViewModel mwvm)
        {
            _listener.Clear();
        }
        ChatListBox.ItemsSource = null;
        ChatListBox.Items.Clear();
        ChatListBox.ItemsSource = _listener.Messages;
    }
}