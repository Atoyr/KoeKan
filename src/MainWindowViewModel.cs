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
    internal readonly IConfigService ConfigService;
    internal readonly IClientService ClientService;
    internal readonly IListenerService ListenerService;
    internal readonly IWindowService WindowService;

    private readonly CommandManager _commandManager = new();
    private readonly CommandFactory _commandFactory;

    /// <summary>
    /// MOD KEY
    /// </summary>
    internal uint ModKey
    {
        get
        {
            return ModKeyExtension.GetModKey(ConfigService.GetConfig().ModKey).ToUInt();
        }
    }

    /// <summary>
    /// HOT KEY
    /// </summary>
    internal uint Key
    {
        get
        {
            return KeyExtension.GetKey(ConfigService.GetConfig().Key).ToUInt();
        }
    }

    internal double Width
    {
        get => ConfigService.GetConfig().Width;
        set
        {
            ConfigService.GetConfig().Width = value;
            ConfigService.SaveConfig();
        }
    }
    internal double Height
    {
        get => ConfigService.GetConfig().Height;
        set
        {
            ConfigService.GetConfig().Height = value;
            ConfigService.SaveConfig();
        }
    }

    // アクティブに変更できるアプリケーション一覧
    internal IEnumerable<string> Applications
    {
        get
        {
            return ConfigService.GetConfig().Applications;
        }
    }

    public MainWindowViewModel(IConfigService configService,
                               IClientService clientService,
                               IListenerService listenerService,
                               IWindowService windowService)
    {
        ConfigService = configService;
        ClientService = clientService;
        ListenerService = listenerService;
        WindowService = windowService;

        // CommandFactoryの初期化
        _commandFactory = new CommandFactory(
            ConfigService,
            ClientService,
            ListenerService,
            WindowService);

        // CommandManagerの初期化
        _commandFactory.InitializeCommandManager(_commandManager);

        var listener = ListenerService.GetListener();
        BindingOperations.EnableCollectionSynchronization(listener.Messages, new object());
    }

    /// <summary>
    /// メッセージを送信します。
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    internal async Task SendMessage(string message)
    {
        var client = ClientService.GetClient();
        var config = ConfigService.GetConfig();

        await client.SendMessageAsync(
            new ClientMessage(
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
            ListenerService.AddLogMessage(helpText);
            return;
        }
    }
}