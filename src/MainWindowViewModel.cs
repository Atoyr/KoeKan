using System.Windows.Data;

using Medoz.KoeKan.Clients;
using Medoz.KoeKan.Command;
using Medoz.KoeKan.Services;

namespace Medoz.KoeKan;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindowViewModel
{
    private readonly IConfigService _configService;
    private readonly IClientService _clientService;
    private readonly IListenerService _listenerService;
    private readonly IWindowService _windowService;

    private readonly CommandManager _commandManager = new();
    private readonly CommandFactory _commandFactory;

    internal Action? OpenSettingWindow { get; set; }

    /// <summary>
    /// MOD KEY
    /// </summary>
    internal uint ModKey
    {
        get
        {
            return ModKeyExtension.GetModKey(_configService.GetConfig().ModKey).ToUInt();
        }
    }

    /// <summary>
    /// HOT KEY
    /// </summary>
    internal uint Key
    {
        get
        {
            return KeyExtension.GetKey(_configService.GetConfig().Key).ToUInt();
        }
    }

    internal double Width
    {
        get => _configService.GetConfig().Width;
        set
        {
            _configService.GetConfig().Width = value;
            _configService.SaveConfig();
        }
    }
    internal double Height
    {
        get => _configService.GetConfig().Height;
        set
        {
            _configService.GetConfig().Height = value;
            _configService.SaveConfig();
        }
    }

    // アクティブに変更できるアプリケーション一覧
    internal IEnumerable<string> Applications
    {
        get
        {
            return _configService.GetConfig().Applications;
        }
    }

    public MainWindowViewModel(
        IConfigService configService,
        IClientService clientService,
        IListenerService listenerService,
        IWindowService windowService)
    {
        _configService = configService;
        _clientService = clientService;
        _listenerService = listenerService;
        _windowService = windowService;

        // CommandFactoryの初期化
        _commandFactory = new CommandFactory(
            configService,
            clientService,
            listenerService,
            windowService);

        // CommandManagerの初期化
        _commandFactory.InitializeCommandManager(_commandManager);

        var listener = _listenerService.GetListener();
        BindingOperations.EnableCollectionSynchronization(listener.Messages, new object());
    }

    /// <summary>
    /// メッセージを送信します。
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    internal async Task SendMessage(string message)
    {
        var client = _clientService.GetClient();
        var config = _configService.GetConfig();

        await client.SendMessageAsync(
            new Message(
                "_",
                "default",
                config.Username,
                message,
                config.Icon));
    }

    /// <summary>
    /// コマンドを実行します。
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    internal async Task ExecuteCommand(string str)
    {
        if (!await _commandManager.TryExecuteCommandAsync(str))
        {
            // コマンドが存在しない場合はヘルプを表示
            var command = str.Split(' ')[0];
            var helpText = _commandManager.GetHelpText(command);
            _listenerService.AddLogMessage(helpText);
            return;
        }
    }
}