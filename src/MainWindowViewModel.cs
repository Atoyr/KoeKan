using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Data;

using Microsoft.Extensions.Logging;

using Medoz.MessageTransporter.Clients;
using Medoz.MessageTransporter.Data;
using System.Windows.Threading;

namespace Medoz.MessageTransporter;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindowViewModel
{
    public ObservableCollection<ChatMessage> Messages = new();

    private ILogger? _logger;
    private Config _config;

    private ITextClient? _activeClient;

    public Action? Close { get; set; }
    public Action? ToggleMoveWindow { get; set; }
    public Action<double, double>? WindowSize { get; set; }
    public Dispatcher? Dispatcher { get; set; }

    // HOT KEY
    public uint ModKey { get => ModKeyExtension.GetModKey(_config.ModKey).ToUInt(); }
    public uint Key { get => KeyExtension.GetKey(_config.Key).ToUInt(); }

    public double Width { get; set; }
    public double Height { get; set; }

    public IEnumerable<string> Applications
    {
        get => _config.Applications;
    }

    public MainWindowViewModel()
    {
        _config = Config.Load() ?? new Config();
        Width = _config.Width;
        Height = _config.Height;
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
            default:
                HelpCommand(command);
                break;
        }
    }

    private void WriteCommand(string arg)
    {
        if (arg == "config")
        {
            _config.Save();
            AddLogMessage(ChatMessageType.LogSuccess, "Save config successed.");
        }
        else
        {
            // TODO ERROR

        }
    }

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
                        var w = Convert.ToDouble(wh[0]);
                        var h = Convert.ToDouble(wh[1]);
                        WindowSize?.Invoke(w, h);
                        _config.Width = w;
                        _config.Height = h;
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
        switch(strs[0])
        {
            case "username":
                _config.Username = arg;
                break;
            case "icon":
                _config.Icon = arg;
                break;
            case "discord.token":
                var secret = Secret.Load();
                secret.EncryptDiscord(arg);
                secret.Save();
                break;
            case "discord.defaultChannel":
                ulong? id = null;
                try
                {
                    id = Convert.ToUInt64(arg);
                }
                catch
                {
                    AddLogMessage(ChatMessageType.LogWarning, "defaultChannel is ulong value.");
                    return;
                }
                _config.Discord = _config.Discord with { DefaultChannelId = id};
                break;
            case "application":
                _config.Applications = _config.Applications.Concat(new string[] { arg });
                break;
            default:
                // TODO HELP MESSAGE
                AddLogMessage(ChatMessageType.LogWarning, "command not found.");
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
        sb.AppendLine("  window     : window command");
        sb.AppendLine("  discord    : discord command");
        sb.AppendLine("  twitch     : twitch command");
        sb.AppendLine("  voicevox   : voicevox command");
        AddLogMessage(ChatMessageType.LogInfo, sb.ToString());
    }

    private void AddLogMessage(ChatMessageType chatMessageType, string message)
        => AddMessage(new ChatMessage(chatMessageType, "", null, "SYSTEM", message, DateTime.Now, IsMessageOnly(chatMessageType, "", "SYSTEM", DateTime.Now)));

    private void AddCommandMessage(string message)
        => AddMessage(new ChatMessage(ChatMessageType.Command, "", null, "COMMAND", message, DateTime.Now, IsMessageOnly(ChatMessageType.Command, "", "COMMAND", DateTime.Now)));
    private void AddUserMessage(string message, DateTime timestamp)
        => AddMessage(new ChatMessage(ChatMessageType.Text, "", _config.Icon, _config.Username, message, DateTime.Now, IsMessageOnly(ChatMessageType.Text, "", _config.Username, timestamp)));

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