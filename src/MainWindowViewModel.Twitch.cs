using Medoz.MessageTransporter.Clients;
using Medoz.MessageTransporter.Data;

namespace Medoz.MessageTransporter;

public partial class MainWindowViewModel
{
    private TwitchClient? _twitchClient;
    private VoicevoxClient? _twitchVoicevoxClient;

    private void SetTwitchClient()
    {
        _twitchClient = new TwitchClient(_config.Twitch.ToTwitchOptions());
        _twitchClient.OnReceiveMessage += ((message) => {
            AddMessage(message);
            return Task.CompletedTask;
        });
        _twitchClient.OnReady += (() => {
            AddLogMessage(ChatMessageType.LogInfo, "Twitch is ready.");
            return Task.CompletedTask;
        });
        
        Task.Run(() =>_twitchClient.RunAsync());
    }

    private async Task TwitchCommand(string arg)
    {
        var strs = arg.Split(' ');
        if (strs[0] == "start" && strs.Length == 1)
        {
            SetTwitchClient();
            AddLogMessage(ChatMessageType.LogInfo, "Start Twitch Connections");
            return;
        }
        await Task.CompletedTask;
    }

}