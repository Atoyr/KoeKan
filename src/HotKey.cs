using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Input;

namespace Medoz.KoeKan;

/// <summary>
/// </summary>
public class HotKey :IDisposable
{
    [DllImport("user32.dll")]
    extern static bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    extern static int UnregisterHotKey(IntPtr HWnd, int ID);

    private const int WM_HOTKEY = 0x0312;
    private int _id = 0;
    private readonly IntPtr _hWnd;
    private readonly HwndSource? _hwndSource;

    public event EventHandler? OnHotKeyPush;

    // https://learn.microsoft.com/ja-jp/windows/win32/api/winuser/nf-winuser-registerhotkey
    // https://learn.microsoft.com/ja-jp/windows/win32/inputdev/virtual-key-codes
    public HotKey(uint modKey, uint key, Window window)
    {
        var helper = new WindowInteropHelper(window);
        _hWnd = helper.Handle;
        _hwndSource = HwndSource.FromHwnd(_hWnd);

        for (int i = 0x0000; i <= 0xbfff; i++)
        {
            if (RegisterHotKey(_hWnd, i, modKey, key))
            {
                _id = i;
                _hwndSource.AddHook(WndProc);
                return;
            }
        }
        if (_id == 0)
        {
            throw new Exception("Register HotKey is false.");
        }
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_HOTKEY)
        {
            if((int)wParam == _id)
            {
                InvokeHotKeyPush();
            }
        }
        return IntPtr.Zero;
    }

    private void InvokeHotKeyPush()
    {
        if(OnHotKeyPush != null)
        {
            OnHotKeyPush(this, EventArgs.Empty);
        }
    }

    public void Dispose()
    {
        UnregisterHotKey(_hWnd, _id);
        _hwndSource?.RemoveHook(WndProc);
    }
}