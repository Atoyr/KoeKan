using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Data;
using System.IO;

using Microsoft.Extensions.Logging;

using Medoz.KoeKan.Clients;
using Medoz.KoeKan.Data;
using Medoz.KoeKan.Command;
using Medoz.Logging;
using System.Windows.Input;

namespace Medoz.KoeKan;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindowViewModel
{
    /// <summary>
    /// メッセージのリスナー
    /// </summary>
    public Listener? Listener { get; set; }

    private ILogger? _logger;

    private readonly string _defaultClient = "default";

    private readonly Dictionary<string, ITextClient> _clients = new();

    /// <summary>
    /// 設定ファイル
    /// </summary>
    private readonly Config _config;

    private readonly RootCommand _rootCommand = new RootCommand();


    /// <summary>
    /// MOD KEY
    /// </summary>
    public uint ModKey
    {
        get
        {
            return ModKeyExtension.GetModKey(_config.ModKey).ToUInt();
        }
    }

    /// <summary>
    /// HOT KEY
    /// </summary>
    public uint Key
    {
        get
        {
            return KeyExtension.GetKey(_config.Key).ToUInt();
        }
    }

    public double Width
    {
        get => _config.Width;
        set
        {
            _config.Width = value;
            _config.Save();
        }
    }
    public double Height
    {
        get => _config.Height;
        set
        {
            _config.Height = value;
            _config.Save();
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

    public MainWindowViewModel()
    {
        _config = Config.Load();
        BindingOperations.EnableCollectionSynchronization(Listener?.Messages, new object());

        AddDefaultClient();
    }

    private void AddDefaultClient()
    {
        var client = new EchoClient(new EchoOptions());
        client.OnReceiveMessage += message => {
            Listener?.AddMessage(message);
            return Task.CompletedTask;
        };
        _clients.Add(_defaultClient, client);
        Listener?.AddMessageConverter(
            _defaultClient,
            (message) => new ChatMessage(
                ChatMessageType.Echo,
                "",
                _config.Icon,
                _config.Username,
                message.Content,
                message.Timestamp,
                false));
    }

    private void AddUICommand()
    {
        _rootCommand.AddCommand("q", new QCommand(this));
        _rootCommand.AddCommand("window", new WindowCommand(this));
    }

    public async Task SendMessage(string message)
    {
        if (!_clients.ContainsKey(_defaultClient))
        {
            throw new InvalidOperationException("Default client is not found.");
        }

        await _clients[_defaultClient].SendMessageAsync(
            new Message(
                _defaultClient,
                "default",
                _config.Username,
                message,
                _config.Icon));
    }

    public CommandArgs CreateCommandArgs(string[] args)
    {
        return new CommandArgs(args, _config, Listener, _clients);
    }

    // コマンド実行
    public async Task ExecuteCommand(string str)
    {
        var split = str.Split(' ');
        var command = split[0];

        var args = new CommandArgs(split, _config, Listener, _clients);
        await _rootCommand.ExecuteCommandAsync(args);

        switch(command)
        {
            case "set":
                SetCommand(args);
                break;
            case "discord":
                await DiscordCommand(args);
                break;
            case "twitch":
                await TwitchCommand(arg);
                break;
            case "clear":
                ClearCommand(arg);
                break;
            case "server":
                Console.WriteLine("Server Command");
                StartWebServer();
                break;
            default:
                HelpCommand(command);
                break;
        }
    }


    private void WriteCommand(string arg)
    {
        if (arg == "config")
        {
            var config = Config.Load();
            config.Save();
            Listener.AddLogMessage(ChatMessageType.LogSuccess, "Save config successed.");
        }
        else
        {
            // TODO ERROR
            Listener.AddLogMessage(ChatMessageType.LogWarning, "Error Save Config is unsuccessed.");
        }
    }

    private void SetCommand(string text)
    {
        var strs = text.Split(' ', 2);
        var arg = strs.Length == 2 ? strs[1] : "";
        var config = Config.Load();

        switch(strs[0])
        {
            case "username":
                config.Username = arg;
                break;
            case "icon":
                config.Icon = arg;
                break;
            case "discord.token":
                var secret = Secret.Load();
                secret.EncryptDiscord(arg);
                secret.Save();
                break;
            case "discord.defaultChannel":
                ulong? discordDefaultChannelId = null;
                try
                {
                    discordDefaultChannelId = Convert.ToUInt64(arg);
                }
                catch
                {
                    Listener.AddLogMessage(ChatMessageType.LogWarning, "defaultChannel is ulong value.");
                    return;
                }
                config.Discord = config.Discord with { DefaultChannelId = discordDefaultChannelId};
                break;
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
            case "log":
                var folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ApplicationInfo.ApplicationName, "config");
                _logger = LoggerUtility.GetLoggerFactory(new FileLoggerSettings(folderPath){ FileName = "log.txt"}).CreateLogger("Medoz");
                Listener.AddLogMessage(ChatMessageType.LogInfo, "Start Logging.");
                break;
            case "nolog":
                Listener.AddLogMessage(ChatMessageType.LogInfo, "Stop Logging.");
                _logger = null;
                break;
            default:
                // 設定画面を開く
                OpenSettingWindow?.Invoke();
                break;
        }
    }

    private void HelpCommand(string arg)
    {
        if (!string.IsNullOrEmpty(arg))
        {
            Listener.AddLogMessage(ChatMessageType.LogWarning, $"⚠️COMMAND {arg} is not found.");
        }

        StringBuilder sb = new();
        sb.AppendLine("COMMAND LIST");
        sb.AppendLine("  w          : write");
        sb.AppendLine("  q          : quit application");
        sb.AppendLine("  set        : set config value");
        sb.AppendLine("  clear      : clear messages");
        sb.AppendLine("  window     : window command");
        sb.AppendLine("  discord    : discord command");
        sb.AppendLine("  twitch     : twitch command");
        sb.AppendLine("  voicevox   : voicevox command");
        Listener.AddLogMessage(ChatMessageType.LogInfo, sb.ToString());
    }

    private void ClearCommand(string arg)
    {
        if (!string.IsNullOrEmpty(arg))
        {
            Listener.AddLogMessage(ChatMessageType.LogWarning, $"⚠️ clear command is not used argument: {arg}.");
            return;
        }
        Listener.ClearMessage();
    }
}