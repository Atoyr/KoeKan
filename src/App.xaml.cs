using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Medoz.CatChast.Messaging;
using Medoz.KoeKan.Services;
using Medoz.KoeKan.Data;

namespace Medoz.KoeKan;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    private readonly IHost _host;
    private Mutex? _mutex;
    private const string mutexName = "CatChast";
    public ApplicationCoordinator? Coordinator { get; private set; }

    public App()
    {
        _host = CreateHostBuilder()
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddEventBusLogger(config =>
                {
                    config.MinLogLevel = LogLevel.Information;
                    config.EnableConsoleOutput = true;
                    // config.OutputPath = "logs"; // ログファイルの出力先を指定する場合はここで設定
                });
                // 他のロガー設定があればここに追加
            })
            .Build();

        this.DispatcherUnhandledException += HandleException;
    }

    private static IHostBuilder CreateHostBuilder() =>
        Host.CreateDefaultBuilder()
            .ConfigureServices((hostContext, services) =>
            {
                // windowの登録
                services.AddTransient<MainWindow>();
                services.AddTransient<SettingsWindow>();

                // メッセージングの登録
                services.AddSingleton<IAsyncEventBus, AsyncEventBus>();

                // サービスの登録
                // NOTE: サービスはアプリケーション内で使い回すことが想定されるため、Singletonとして登録
                services.AddSingleton<IClientService, ClientService>();
                services.AddSingleton<ISpeakerService, SpeakerService>();
                services.AddSingleton<IConfigService, ConfigService>();
                services.AddSingleton<IWindowService, WindowService>();
                services.AddSingleton<IServerService, ServerService>();

                // ViewModelの登録
                services.AddTransient<MainWindowViewModel>();
                services.AddTransient<SettingsWindowViewModel>();
            });

    public void HandleException(
            object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"Handled: {e.Exception.Message}");
        var logger = LoggerUtility.GetLoggerFactory().CreateLogger("UnhandledException");
        logger.LogError(e.Exception, "UnhandledException");
        e.Handled = true;
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();
        // 重複起動を防ぐ
        _mutex = new(true, mutexName, out bool createdNew);

        if (!createdNew)
        {
            // 既に起動している場合は既存のプロセスを前面に表示
            ActivateExistingInstance();
            Current.Shutdown();
            return;
        }

        // シャットダウンモードを明示的に設定
        this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        Coordinator = new ApplicationCoordinator(
            _host.Services.GetRequiredService<IWindowService>());

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

    protected override async void OnExit(ExitEventArgs e)
    {
        using (_host)
        {
            await _host.StopAsync();
        }
        _mutex?.Dispose();
        base.OnExit(e);
    }

    public static T GetInstance<T>() where T : class
    {
        if (Current is not App app)
        {
            throw new InvalidOperationException("App.Current is not initialized or not of type App");
        }
        if (app._host == null)
        {
            throw new InvalidOperationException("Host is not initialized");
        }
        return app._host.Services.GetRequiredService<T>();
    }


    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);
}