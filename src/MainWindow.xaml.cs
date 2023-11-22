using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Medoz.TextTransporter;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    [DllImport("user32.dll", SetLastError = true)]
    static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    const UInt32 SWP_NOSIZE = 0x0001;
    const UInt32 SWP_NOMOVE = 0x0002;

    private HotKey? _hk;
    private Config _config;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        this._hk = new HotKey(0x0004, 0x31, this);
        _hk.OnHotKeyPush += Focus;
        var helper = new WindowInteropHelper(this);
        SetWindowPos(helper.Handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
    }

    public void Focus(object sender, EventArgs e)
    {
        myTextBox.Focus();
        this.Activate();
    }

    public void KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            ChatListBox.Items.Add(myTextBox.Text);
            myTextBox.Text = "";
            ActivateOtherWindow();
        }
    }

    public void ActivateOtherWindow()
    {
        // プロセス名を指定
        var processName = "PAYDAY3Client-Win64-Shipping";
        var processes = Process.GetProcessesByName(processName);
        if (processes.Length > 0)
        {
            var process = processes[0]; // 最初のプロセスを使用
            IntPtr windowHandle = process.MainWindowHandle;
            SetForegroundWindow(windowHandle);
        }
    }
}