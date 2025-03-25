using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Data;
using System.Windows.Threading;
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
    public Listener Listener { get; set; } = new();

    private Microsoft.Extensions.Logging.ILogger? _logger;

    private readonly string _defaultClient = "default";

    private readonly Dictionary<string, ITextClient> _clients = new();

    private ITextClient? _activeClient;

    private readonly Config _config;

    private readonly RootCommand _rootCommand = new RootCommand();


    // View Action
    public Action? Close { get; set; }
    public Action? ToggleMoveWindow { get; set; }
    public Action<double, double>? WindowSize { get; set; }
    public Dispatcher? Dispatcher { get; set; }
    public Action? OpenSettingWindow { get; set; }

    // HOT KEY
    public uint ModKey
    {
        get
        {
            var config = Config.Load();
            return ModKeyExtension.GetModKey(config.ModKey).ToUInt();
        }
    }

    public uint Key
    {
        get
        {
            var config = Config.Load();
            return KeyExtension.GetKey(config.Key).ToUInt();
        }
    }

    public double Width { get; set; }
    public double Height { get; set; }

    // アクティブに変更できるアプリケーション一覧
    public IEnumerable<string> Applications
    {
        get
        {
            var config = Config.Load();
            return config.Applications;
        }
    }

    public MainWindowViewModel()
    {
        _config = Config.Load();
        Width = _config.Width;
        Height = _config.Height;
        BindingOperations.EnableCollectionSynchronization(Listener.Messages, new object());

        var client = new EchoClient(new EchoOptions());
        client.OnReceiveMessage += ((message) => {
            Listener.AddMessage(message);
            return Task.CompletedTask;
        });
        _clients.Add(_defaultClient, client);
        Listener.AddMessageConverter(
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

    public async Task SendMessage(string message)
    {
        using var _ = _clients[_defaultClient].SendMessageAsync(
            new Message(
                _defaultClient,
                "default",
                _config.Username,
                message,
                _config.Icon));
        await Task.CompletedTask;
    }

    // コマンド実行
    public async Task ExecuteCommand(string str)
    {
        var split = str.Split(' ', 2);
        var command = split[0];
        string arg = "";
        if (split.Length > 1)
        {
            arg = split[1];
        }

        switch(command)
        {
            case "w":
                WriteCommand(arg);
                break;
            case "q":
                QuitCommand(arg);
                break;
            case "set":
                SetCommand(arg);
                break;
            case "window":
                WindowCommand(arg);
                break;
            case "discord":
                await DiscordCommand(arg);
                break;
            case "twitch":
                await TwitchCommand(arg);
                break;
            case "voicevox":
                var args = new CommandArgs(split, _config, Listener, _clients);
                await _rootCommand.ExecuteCommandAsync(args);
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

    // ウィンドウ操作に関連するコマンド
    private void WindowCommand(string arg)
    {
        var strs = arg.Split(' ', 2);
        var args = strs.Length == 2 ? strs[1] : "";
        switch (strs[0])
        {
            case "move":
                ToggleMoveWindow?.Invoke();
                Listener.AddLogMessage(ChatMessageType.LogInfo, "Toggle Move Window.");
                break;
            case "size":
                var wh = args.Split(' ');
                if (wh.Length == 2)
                {
                    try
                    {
                        var config = Config.Load();
                        var w = Convert.ToDouble(wh[0]);
                        var h = Convert.ToDouble(wh[1]);
                        WindowSize?.Invoke(w, h);
                        config.Width = w;
                        config.Height = h;
                        Listener.AddLogMessage(ChatMessageType.LogSuccess, $"Change Window Size. Widht:{w} Height:{h}");
                    }
                    catch
                    {
                        Listener.AddLogMessage(ChatMessageType.LogWarning, "Width or Height is not double value.");
                    }
                }
                else
                {
                    Listener.AddLogMessage(ChatMessageType.LogWarning, "args is not validate.");
                }
                break;
            default:
                break;
        }
    }

    private void QuitCommand(string arg)
    {
        if(string.IsNullOrEmpty(arg))
        {
            Close?.Invoke();
        }
        else
        {
            // TODO ERROR
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