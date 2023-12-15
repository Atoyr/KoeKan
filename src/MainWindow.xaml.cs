using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Medoz.MessageTransporter;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// このスタイルで作成されるウィンドウが透明であることを示します。
    /// つまり、このウィンドウより奥にあるすべてのウィンドウは、このウィンドウによって隠されることはありません。
    /// このスタイルで作成したウィンドウは、自らより奥にあるすべての兄弟ウィンドウが更新された後でのみ、WM_PAINT メッセージを受信します。
    /// </summary>
    private const UInt32 WS_EX_TRANSPARENT = 0x00000020;

    /// <summary>Sets a new extended window style.</summary>
    private const Int32 GWL_EXSTYLE = -20;

    private const UInt32 SWP_NOSIZE = 0x0001;
    private const UInt32 SWP_NOMOVE = 0x0002;

    private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

 
    [DllImport("user32.dll", SetLastError = true)]
    static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    /// <summary>ウインドウスタイルの取得</summary>
    [DllImport("user32.dll")]
    private static extern UInt32 GetWindowLong(IntPtr hWnd, Int32 index);
 
    /// <summary>ウインドウスタイルの設定</summary>
    [DllImport("user32.dll")]
    private static extern UInt32 SetWindowLong(IntPtr hWnd, Int32 index, UInt32 newLong);

    private UInt32 _defaultStyle;

    private HotKey? _hk;

    private SemaphoreSlim _messageSemaphore = new(1, 1);

    private string? _activeProcessName;

    public MainWindow()
    {
        InitializeComponent();

        SourceInitialized += ((sender, e) => {
            var handle = new WindowInteropHelper(this).Handle;
            UInt32 style = GetWindowLong(handle, GWL_EXSTYLE);
            _defaultStyle = style;
            SetWindowLong(handle, GWL_EXSTYLE, style | WS_EX_TRANSPARENT);
        });

        DataContext = new MainWindowViewModel();
        MainWindowViewModel mwvm = (MainWindowViewModel)DataContext;
        mwvm.WindowSize = (w, h) => {
            Width = w;
            Height = h;
        };
        Width = mwvm.Width;
        Height = mwvm.Height;
        mwvm.Close = Close;
        mwvm.Dispatcher = Dispatcher;
        mwvm.ToggleMoveWindow = () => {
            var handle = new WindowInteropHelper(this).Handle;
            switch (MoveWindowBar.Visibility)
            {
                case Visibility.Visible:
                    MoveWindowBar.Visibility = Visibility.Collapsed;
                    UInt32 style = GetWindowLong(handle, GWL_EXSTYLE);
                    SetWindowLong(handle, GWL_EXSTYLE, style | WS_EX_TRANSPARENT);
                    ChatListBox.SelectedItem = null;
                    break;
                case Visibility.Collapsed:
                    MoveWindowBar.Visibility = Visibility.Visible;
                    SetWindowLong(handle, GWL_EXSTYLE, _defaultStyle);
                    break;
            }
        };
        mwvm.Messages.CollectionChanged += (_, e) => {
            Dispatcher.BeginInvoke( new Action(() => { ChatListBox_ScrollToEnd(); }), System.Windows.Threading.DispatcherPriority.ContextIdle);
        };
        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        MainWindowViewModel mwvm = (MainWindowViewModel)DataContext;
        _hk = new HotKey(mwvm.ModKey, mwvm.Key, this);
        _hk.OnHotKeyPush += MessageBox_Focus;

        ChatListBox.ItemsSource = ((MainWindowViewModel)DataContext).Messages;
        MoveWindowBar.Visibility = Visibility.Collapsed;
    }

    public void ActivateOtherWindow()
    {
        if (_activeProcessName is not null && ActivateOtherWindow(_activeProcessName))
        {
            return;
        }
        
        MainWindowViewModel mwvm = (MainWindowViewModel)DataContext;
        _activeProcessName = null;
        // プロセス名を指定
        // var processName = "PAYDAY3Client-Win64-Shipping";
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

    public async void MessageBox_KeyDown(object sender, KeyEventArgs e)
    {

        if (e.Key == Key.Enter)
        {
            if (!await _messageSemaphore.WaitAsync(0))
            {
                return;
            }

            try
            {
                if (string.IsNullOrWhiteSpace(MessageBox.Text))
                {
                }
                else if (MessageBox.Text[0] == ':') 
                {
                    await ((MainWindowViewModel)DataContext).ExecuteCommand(MessageBox.Text.Substring(1));
                }
                else 
                {
                    await ((MainWindowViewModel)DataContext).SendMessage(MessageBox.Text);
                }
                MessageBox.Text = "";
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

    private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        => DragMove();

    private void SetWindowIsTops()
    {
        var helper = new WindowInteropHelper(this);
        SetWindowPos(helper.Handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
    }

    private void ChatListBox_ScrollToEnd()
    {
        var item = ChatListBox.Items[ChatListBox.Items.Count - 1];
        ChatListBox.ScrollIntoView(item);
    }

}