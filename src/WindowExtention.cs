using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Medoz.KoeKan;

public static class WindowExtention
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

    /// <summary>ウインドウスタイルの取得</summary>
    [DllImport("user32.dll")]
    private static extern UInt32 GetWindowLong(IntPtr hWnd, Int32 index);

    /// <summary>ウインドウスタイルの設定</summary>
    [DllImport("user32.dll")]
    private static extern UInt32 SetWindowLong(IntPtr hWnd, Int32 index, UInt32 newLong);

    /// <summary>
    /// ウィンドウを最前面に表示します。
    /// </summary>
    /// <param name="window">ウィンドウ</param>
    public static void SetTopMost(this Window window)
    {
        var handle = new WindowInteropHelper(window).Handle;
        SetWindowPos(handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
    }

    /// <summary>
    /// ウィンドウを通常の位置で表示します
    /// </summary>
    /// <param name="window"></param>
    public static void ShowWindow(this Window window)
    {
        window.Show();
        window.ShowInTaskbar = true;
        window.Activate();
    }

    /// <summary>
    /// ウィンドウを非表示にします
    /// </summary>
    /// <param name="window"></param>
    public static void HideWindow(this Window window)
    {
        window.Hide();
        window.ShowInTaskbar = false;
    }

    /// <summary>
    /// ウィンドウを閉じます
    /// </summary>
    /// <param name="window"></param>
    public static void CloseWindow(this Window window)
    {
        window.Close();
        window.ShowInTaskbar = false;
    }

    public static bool IsWindowTransparent(this Window window)
    {
        var handle = new WindowInteropHelper(window).Handle;
        UInt32 style = GetWindowLong(handle, GWL_EXSTYLE);
        return (style & WS_EX_TRANSPARENT) == WS_EX_TRANSPARENT;
    }

    public static void ToggleWindowTransparent(this Window window)
    {
        if (window.IsWindowTransparent())
        {
            window.SetWindowTransparent(false);
        }
        else
        {
            window.SetWindowTransparent(true);
        }
    }

    public static void SetWindowTransparent(this Window window, bool transparenting = false)
    {
        var handle = new WindowInteropHelper(window).Handle;
        UInt32 style = GetWindowLong(handle, GWL_EXSTYLE);
        if (transparenting)
        {
            style |= WS_EX_TRANSPARENT;
        }
        else
        {
            style &= ~WS_EX_TRANSPARENT;
        }
        SetWindowLong(handle, GWL_EXSTYLE, style);
    }

    public static void SetWindowSize(this Window window, double width, double height)
    {
        window.Width = width;
        window.Height = height;
    }

    public static void SetWindowPosition(this Window window, double x, double y)
    {
        window.Left = x;
        window.Top = y;
    }
}