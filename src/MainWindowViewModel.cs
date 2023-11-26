using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Data;

using Microsoft.Extensions.Logging;

using Medoz.MessageTransporter.Clients;
using Medoz.MessageTransporter.Data;

namespace Medoz.MessageTransporter;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindowViewModel
{
    public ObservableCollection<ChatMessage> Messages = new();

    private ILogger? _logger;
    private Config _config;

    private DiscordClient? _discordClient;

    public Action? Close { get; set; }

    public MainWindowViewModel()
    {
        _config = Config.Load() ?? new Config();
        BindingOperations.EnableCollectionSynchronization(Messages, new object());
    }

    private void SetDiscordClient()
    {
        using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
        _discordClient = new DiscordClient(_config.Discord.ToDiscordOptions(), factory.CreateLogger<MainWindow>());
        _discordClient.OnReceiveMessage += ((message) => {
            Messages.Add(new ChatMessage(ChatMessageType.DiscordText, message.Channel, message.Username, message.Content, message.Timestamp, message.IconSource));
            return Task.CompletedTask;
        });
        _discordClient.OnReady += (() => {
            SetSystemMessage("Discord is ready.");
            return Task.CompletedTask;
        });
        Task.Run(() =>_discordClient.RunAsync());
    }

    public async Task SendMessage(string message)
    {
        if (_discordClient is not null)
        {
            await _discordClient.SendMessageAsync(message);
        }
        Messages.Add(new ChatMessage(ChatMessageType.Text, "", "", message, DateTime.Now, null));
    }

    public async Task ExecuteCommand(string str)
    {
        var split = str.Split(' ', 2);
        var command = split[0];
        string text = "";
        if (split.Length > 1)
        {
            text = split[1];
        }

        switch(command)
        {
            case "w":
                WriteCommand(text);
                break;
            case "q":
                QuitCommand(text);
                break;
            case "discord":
                await DiscordCommand(text);
                break;
        }
    }

    private void WriteCommand(string text)
    {
        if (text == "config")
        {
            _config.Save();
            SetSystemMessage("Save config successed.");
        }
        else
        {
            // TODO ERROR

        }
    }

    private void QuitCommand(string text)
    {
        if(string.IsNullOrEmpty(text))
        {
            Close();
        }
        else
        {
            // TODO ERROR
        }
    }

    private async Task DiscordCommand(string text)
    {
        var strs = text.Split(' ');
        if (strs[0] == "start" && strs.Length == 1)
        {
            SetDiscordClient();
            SetSystemMessage("Start Discord Connections");
            return;
        }

        if (_discordClient is null)
        {
            SetSystemMessage("Discord is not started");
            return;
        }
        if (strs[0] == "channels" && strs.Length == 1)
        {
            StringBuilder sb = new();
            foreach(var c in _discordClient.GetChannels())
            {
                sb.AppendLine($"{c.GuildName} | {c.Id} : {c.Name}");
            }
            SetCommandMessage(sb.ToString());
        }
        else if (strs[0] == "channel" && strs.Length == 2)
        {
            if (ulong.TryParse(strs[1], out ulong id))
            {
                _discordClient.SetChannel(id);
            }
            else
            {
                SetCommandMessage("Id is not validated");
            }
        }
        else if (strs[0] == "guilds" && strs.Length == 1)
        {
            var guilds = await _discordClient.GetGuildsAsync();
            StringBuilder sb = new();
            foreach(var g in guilds)
            {
                sb.AppendLine($"{g.Id} : {g.Name}");
            }
            SetCommandMessage(sb.ToString());
        }
    }

    private void SetSystemMessage(string message)
        => Messages.Add(new ChatMessage(ChatMessageType.System, "", "", message, DateTime.Now, null));
    private void SetCommandMessage(string message)
        => Messages.Add(new ChatMessage(ChatMessageType.Command, "", "", message, DateTime.Now, null));


}