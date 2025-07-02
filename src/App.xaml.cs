using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

using Microsoft.Extensions.Logging;

namespace Medoz.KoeKan;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    private System.Threading.Mutex? _mutex;
    public ApplicationCoordinator? Coordinator { get; private set; }

    public void HandleException(
            object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"Handled: {e.Exception.Message}");
        var logger = LoggerUtility.GetLoggerFactory().CreateLogger("UnhandledException");
        logger.LogError(e.Exception, "UnhandledException");
        e.Handled = true;
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        // 重複起動を防ぐ
        const string mutexName = "YourWPFApp_SingleInstance";
        _mutex = new System.Threading.Mutex(true, mutexName, out bool createdNew);

        if (!createdNew)
        {
            // 既に起動している場合は既存のプロセスを前面に表示
            ActivateExistingInstance();
            Current.Shutdown();
            return;
        }

        // シャットダウンモードを明示的に設定
        this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        Coordinator = new ApplicationCoordinator();

        base.OnStartup(e);
    }
    private void ActivateExistingInstance()
    {
        try
        {
            var currentProcess = Process.GetCurrentProcess();
            var processes = Process.GetProcessesByName(currentProcess.ProcessName);

            var existingProcess = processes.FirstOrDefault(p => p.Id != currentProcess.Id);
            if (existingProcess != null)
            {
                // 既存のプロセスのウィンドウを前面に表示
                ShowWindow(existingProcess.MainWindowHandle, 9); // SW_RESTORE
                SetForegroundWindow(existingProcess.MainWindowHandle);
            }
        }
        catch
        {
            // エラーが発生した場合は何もしない
        }
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);
}