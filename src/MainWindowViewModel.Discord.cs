using System.Text;
using System.IO;

using Medoz.KoeKan.Clients;
using Medoz.KoeKan.Data;

namespace Medoz.KoeKan;

public partial class MainWindowViewModel
{
    private DiscordClient? _discordClient;
    private VoicevoxClient? _discordVoicevoxClient;

    private void SetDiscordClient()
    {
        var config = Config.Load();
        var token = Secret.Load().DectyptDiscord();
        if (token is null)
        {
            throw new NullReferenceException();
        }

        _discordClient = new DiscordClient(new DiscordOptions(){Token = token });
        _discordClient.OnReceiveMessage += ((message) => {
            Listener.AddMessage(message);
            _discordVoicevoxClient?.SpeakMessageAsync(message.Content);
            return Task.CompletedTask;
        });
        _discordClient.OnReady += (async () => {
            // _activeClient = _discordClient;

            if (config.Discord.DefaultChannelId is ulong id)
            {
                _discordClient.SetChannel(id);
            }

            Listener.AddLogMessage(ChatMessageType.LogInfo, "Discord is ready.");
        });
        Task.Run(() =>_discordClient.RunAsync());
    }

    private async Task DiscordCommand(string arg)
    {
        var strs = arg.Split(' ');
        if (strs[0] == "start" && strs.Length == 1)
        {
            SetDiscordClient();
            Listener.AddLogMessage(ChatMessageType.LogInfo, "Start Discord Connections");
            return;
        }

        if (_discordClient is null)
        {
            Listener.AddLogMessage(ChatMessageType.LogWarning, "Discord is not started");
            return;
        }
        if (strs[0] == "channels" && strs.Length == 1)
        {
            StringBuilder sb = new();
            foreach(var c in _discordClient.GetChannels())
            {
                sb.AppendLine($"{c.GuildName} | {c.Id} : {c.Name}");
            }
            Listener.AddCommandMessage(sb.ToString());
        }
        else if (strs[0] == "channel" && strs.Length == 2)
        {
            if (ulong.TryParse(strs[1], out ulong id))
            {
                _discordClient.SetChannel(id);
            }
            else
            {
                Listener.AddLogMessage(ChatMessageType.LogWarning, "Id is not validated.");
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
            Listener.AddCommandMessage(sb.ToString());
        }
        else if (strs[0] == "voice" && strs.Length == 2)
        {
            if (ulong.TryParse(strs[1], out ulong id))
            {
                Listener.AddLogMessage(ChatMessageType.LogWarning, "voice to text is not impletemt.");
            }
            else
            {
                Listener.AddLogMessage(ChatMessageType.LogWarning, "voice Id is not validated.");
            }
        }
    }
}