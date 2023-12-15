using System.Text;
using System.IO;

using Medoz.MessageTransporter.Clients;
using Medoz.MessageTransporter.Data;

namespace Medoz.MessageTransporter;

public partial class MainWindowViewModel
{
    private DiscordClient? _discordClient;
    private VoicevoxClient? _discordVoicevoxClient;

    private void SetDiscordClient()
    {
        var token = Secret.Load().DectyptDiscord();
        if (token is null)
        {
            throw new NullReferenceException();
        }
        
        _discordClient = new DiscordClient(new DiscordOptions(token), null);
        _discordClient.OnReceiveMessage += ((message) => {
            AddMessage(message);
            _discordVoicevoxClient?.SpeakMessageAsync(message.Content);
            return Task.CompletedTask;
        });
        _discordClient.OnReady += (async () => {
            _activeClient = _discordClient;

            if (_config.Discord.DefaultChannelId is ulong id)
            {
                _discordClient.SetChannel(id);
            }

            AddLogMessage(ChatMessageType.LogInfo, "Discord is ready.");
            if (_config.Discord.UseSpeaker)
            {
                _discordVoicevoxClient = new VoicevoxClient(_config.Discord.Speaker ?? 1, _config.Voicevox.Url);
            AddLogMessage(ChatMessageType.LogInfo, "Discord is used Voicevox and Voicevox is ready.");
                _discordVoicevoxClient.OnReady += (() => {
                    return Task.CompletedTask;
                        });
                await _discordVoicevoxClient.RunAsync();
            }
        });
        Task.Run(() =>_discordClient.RunAsync());
    }

    private async Task DiscordCommand(string arg)
    {
        var strs = arg.Split(' ');
        if (strs[0] == "start" && strs.Length == 1)
        {
            SetDiscordClient();
            AddLogMessage(ChatMessageType.LogInfo, "Start Discord Connections");
            return;
        }

        if (_discordClient is null)
        {
            AddLogMessage(ChatMessageType.LogWarning, "Discord is not started");
            return;
        }
        if (strs[0] == "channels" && strs.Length == 1)
        {
            StringBuilder sb = new();
            foreach(var c in _discordClient.GetChannels())
            {
                sb.AppendLine($"{c.GuildName} | {c.Id} : {c.Name}");
            }
            AddCommandMessage(sb.ToString());
        }
        else if (strs[0] == "channel" && strs.Length == 2)
        {
            if (ulong.TryParse(strs[1], out ulong id))
            {
                _discordClient.SetChannel(id);
            }
            else
            {
                AddLogMessage(ChatMessageType.LogWarning, "Id is not validated.");
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
            AddCommandMessage(sb.ToString());
        }
        else if (strs[0] == "voice" && strs.Length == 2)
        {
            if (ulong.TryParse(strs[1], out ulong id))
            {
                AddLogMessage(ChatMessageType.LogWarning, "voice to text is not impletemt.");
            }
            else
            {
                AddLogMessage(ChatMessageType.LogWarning, "voice Id is not validated.");
            }
        }
    }
}