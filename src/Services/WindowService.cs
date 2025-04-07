using System;
using System.Windows;
using System.Windows.Interop;
using System.Runtime.InteropServices;

using Medoz.KoeKan.Clients;
using Medoz.KoeKan.Data;

namespace Medoz.KoeKan.Services;

public class WindowService : IWindowService
{
    private readonly Window _window;

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

    private UInt32 _defaultStyle;
    private bool _isMoveable = false;

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    /// <summary>ウインドウスタイルの取得</summary>
    [DllImport("user32.dll")]
    private static extern UInt32 GetWindowLong(IntPtr hWnd, Int32 index);

    /// <summary>ウインドウスタイルの設定</summary>
    [DllImport("user32.dll")]
    private static extern UInt32 SetWindowLong(IntPtr hWnd, Int32 index, UInt32 newLong);

    public WindowService(Window window)
    {
        _window = window ?? throw new ArgumentNullException(nameof(window));

        // ウィンドウハンドルが初期化された後にデフォルトスタイルを保存
        if (PresentationSource.FromVisual(window) is HwndSource)
        {
            SaveDefaultStyle();
        }
        else
        {
            window.SourceInitialized += (sender, e) => SaveDefaultStyle();
        }
    }

    private void SaveDefaultStyle()
    {
        var handle = new WindowInteropHelper(_window).Handle;
        _defaultStyle = GetWindowLong(handle, GWL_EXSTYLE);
    }

    public void Close()
    {
        _window.Close();
    }

    public void ToggleMoveableWindow()
    {
        ChangeMoveableWindowState(!_isMoveable);
    }

    public void ChangeMoveableWindowState(bool isMoveable)
    {
        _isMoveable = isMoveable;
        var handle = new WindowInteropHelper(_window).Handle;

        if (isMoveable)
        {
            // 移動可能状態に設定
            SetWindowLong(handle, GWL_EXSTYLE, _defaultStyle);
        }
        else
        {
            // 透明状態に設定
            UInt32 style = GetWindowLong(handle, GWL_EXSTYLE);
            SetWindowLong(handle, GWL_EXSTYLE, style | WS_EX_TRANSPARENT);
        }
    }

    public void SetWindowSize(double width, double height)
    {
        _window.Width = width;
        _window.Height = height;
    }

    public void SetWindowPosition(double x, double y)
    {
        _window.Left = x;
        _window.Top = y;
    }
}