using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Data;
using System.IO;

using Microsoft.Extensions.Logging;

using Medoz.KoeKan.Clients;
using Medoz.KoeKan.Data;
using Medoz.KoeKan.Command;
using Medoz.Logging;
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


    /// <summary>
    /// 設定ファイル
    /// </summary>
    private readonly Config _config;

    private readonly CommandManager _commandManager = new();

    /// <summary>
    /// MOD KEY
    /// </summary>
    public uint ModKey
    {
        get
        {
            return ModKeyExtension.GetModKey(_configService.GetConfig().ModKey).ToUInt();
        }
    }

    /// <summary>
    /// HOT KEY
    /// </summary>
    public uint Key
    {
        get
        {
            return KeyExtension.GetKey(_configService.GetConfig().Key).ToUInt();
        }
    }

    public double Width
    {
        get => _configService.GetConfig().Width;
        set
        {
            _configService.GetConfig().Width = value;
            _configService.SaveConfig();
        }
    }
    public double Height
    {
        get => _configService.GetConfig().Height;
        set
        {
            _configService.GetConfig().Height = value;
            _configService.SaveConfig();
        }
    }

    // アクティブに変更できるアプリケーション一覧
    public IEnumerable<string> Applications
    {
        get
        {
            return _config.Applications;
        }
    }

    public MainWindowViewModel(
        IConfigService configService,
        IClientService clientService,
        IListenerService listenerService)
    {
        _configService = configService;
        _clientService = clientService;
        _listenerService = listenerService;

        var listener = _listenerService.GetListener();
        BindingOperations.EnableCollectionSynchronization(listener.Messages, new object());
    }


    public async Task SendMessage(string message)
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

    // コマンド実行
    public async Task ExecuteCommand(string str)
    {
        var ok = await _commandManager.TryExecuteCommandAsync(str);

        // switch(command)
        // {
        //     case "set":
        //         SetCommand(args);
        //         break;
        //     case "discord":
        //         await DiscordCommand(args);
        //         break;
        //     case "twitch":
        //         await TwitchCommand(arg);
        //         break;
        //     case "clear":
        //         ClearCommand(arg);
        //         break;
        //     case "server":
        //         Console.WriteLine("Server Command");
        //         StartWebServer();
        //         break;
        //     default:
        //         HelpCommand(command);
        //         break;
        // }
    }

    private void SetCommand(string text)
    {
        var strs = text.Split(' ', 2);
        var arg = strs.Length == 2 ? strs[1] : "";
        var config = Config.Load();

        switch(strs[0])
        {
            case "voicevox.speaker":
                uint? voicevoxSpeakerId = null;
                try
                {
                    voicevoxSpeakerId = Convert.ToUInt32(arg);
                }
                catch
                {
                    Listener.AddLogMessage(ChatMessageType.LogWarning, "speaker is uint value.");
                    return;
                }
                // SetVoicevoxSpeakerId((uint)voicevoxSpeakerId);
                config.Voicevox = config.Voicevox with { SpeakerId = (uint)voicevoxSpeakerId};
                break;
            case "application":
                config.Applications = config.Applications.Concat(new string[] { arg });
                break;
            default:
                // 設定画面を開く
                OpenSettingWindow?.Invoke();
                break;
        }
    }
}