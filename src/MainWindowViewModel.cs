using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Data;
using System.Windows.Threading;
using System.IO;

using Microsoft.Extensions.Logging;

using Medoz.KoeKan.Clients;
using Medoz.KoeKan.Data;
using Medoz.Logging;

namespace Medoz.KoeKan;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindowViewModel
{
    // Chat Messages
    public ObservableCollection<ChatMessage> Messages = new();

    private Microsoft.Extensions.Logging.ILogger? _logger;

    private ITextClient? _activeClient;

    private readonly Dictionary<string, ITextClient> _clients = new();

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
        var config = Config.Load();
        Width = config.Width;
        Height = config.Height;
        BindingOperations.EnableCollectionSynchronization(Messages, new object());
    }

    public async Task SendMessage(string message)
    {
        if (_activeClient is not null)
        {
            _activeClient.SendMessageAsync(message);
        }
        if (_voicevoxClient is not null)
        {
            _voicevoxClient.SpeakMessageAsync(message);
        }
        AddUserMessage(message, DateTime.Now);
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
                await VoicevoxCommand(arg);
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
            AddLogMessage(ChatMessageType.LogSuccess, "Save config successed.");
        }
        else
        {
            // TODO ERROR
            AddLogMessage(ChatMessageType.LogWarning, "Error Save Config is unsuccessed.");
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
                AddLogMessage(ChatMessageType.LogInfo, "Toggle Move Window.");
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
                        AddLogMessage(ChatMessageType.LogSuccess, $"Change Window Size. Widht:{w} Height:{h}");
                    }
                    catch
                    {
                        AddLogMessage(ChatMessageType.LogWarning, "Width or Height is not double value.");
                    }
                }
                else
                {
                    AddLogMessage(ChatMessageType.LogWarning, "args is not validate.");
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
                    AddLogMessage(ChatMessageType.LogWarning, "defaultChannel is ulong value.");
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
                    AddLogMessage(ChatMessageType.LogWarning, "speaker is uint value.");
                    return;
                }
                SetVoicevoxSpeakerId((uint)voicevoxSpeakerId);
                config.Voicevox = config.Voicevox with { SpeakerId = (uint)voicevoxSpeakerId};
                break;
            case "application":
                config.Applications = config.Applications.Concat(new string[] { arg });
                break;
            case "log":
                var folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ApplicationInfo.ApplicationName, "config");
                _logger = LoggerUtility.GetLoggerFactory(new FileLoggerSettings(folderPath){ FileName = "log.txt"}).CreateLogger("Medoz");
                AddLogMessage(ChatMessageType.LogInfo, "Start Logging.");
                break;
            case "nolog":
                AddLogMessage(ChatMessageType.LogInfo, "Stop Logging.");
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
            AddLogMessage(ChatMessageType.LogWarning, $"⚠️COMMAND {arg} is not found.");
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
        AddLogMessage(ChatMessageType.LogInfo, sb.ToString());
    }

    private void ClearCommand(string arg)
    {
        if (!string.IsNullOrEmpty(arg))
        {
            AddLogMessage(ChatMessageType.LogWarning, $"⚠️ clear command is not used argument: {arg}.");
            return;
        }
        ClearMessage();
    }


    private void AddLogMessage(ChatMessageType chatMessageType, string message)
        => AddMessage(new ChatMessage(chatMessageType, "", null, "SYSTEM", message, DateTime.Now, IsMessageOnly(chatMessageType, "", "SYSTEM", DateTime.Now)));

    private void AddCommandMessage(string message)
        => AddMessage(new ChatMessage(ChatMessageType.Command, "", null, "COMMAND", message, DateTime.Now, IsMessageOnly(ChatMessageType.Command, "", "COMMAND", DateTime.Now)));
    private void AddUserMessage(string message, DateTime timestamp)
    {
        var config = Config.Load();
        AddMessage(new ChatMessage(ChatMessageType.Text, "", config.Icon, config.Username, message, DateTime.Now, IsMessageOnly(ChatMessageType.Text, "", config.Username, timestamp)));
    }

    private void AddMessage(Message message)
        => AddMessage(new ChatMessage(
                    ChatMessageType.DiscordText,
                    message.Channel,
                    message.IconSource,
                    message.Username,
                    message.Content,
                    message.Timestamp,
                    IsMessageOnly(ChatMessageType.DiscordText, message.Channel, message.Username, message.Timestamp)));

    private void AddMessage(ChatMessage cm)
    {
        if (Dispatcher is null)
        {
            Messages.Add(cm);
        }
        else
        {
            Dispatcher.Invoke(() => Messages.Add(cm));
        }

        if (_logger is not null)
        {
            switch (cm.MessageType)
            {
                case ChatMessageType.LogInfo:
                    _logger.LogInformation(cm.Message);
                    break;
                case ChatMessageType.LogSuccess:
                    _logger.LogInformation(cm.Message);
                    break;
                case ChatMessageType.LogWarning:
                    _logger.LogWarning(cm.Message);
                    break;
                case ChatMessageType.LogFatal:
                    _logger.LogError(cm.Message);
                    break;
                default:
                    break;
            }
        }
    }

    private void ClearMessage()
    {
        if (Dispatcher is null)
        {
            Messages.Clear();
        }
        else
        {
            Dispatcher.Invoke(() => Messages.Clear());
        }
    }

    private bool IsMessageOnly(ChatMessageType chatMessageType, string channel, string username, DateTime timestamp)
    {
        if (Messages.Count == 0)
        {
            return false;
        }
        var last = Messages.Last();
        return (last.MessageType == chatMessageType && last.Channel == channel && last.Username == username && (timestamp - last.Timestamp).TotalMinutes <= 1);
    }
}