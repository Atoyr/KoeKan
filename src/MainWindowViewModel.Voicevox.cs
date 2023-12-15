using Medoz.MessageTransporter.Clients;
using Medoz.MessageTransporter.Data;

namespace Medoz.MessageTransporter;

public partial class MainWindowViewModel
{
    private VoicevoxClient? _voicevoxClient;

    private void SetVoicevoxClient()
    {
        _voicevoxClient = new VoicevoxClient(_config.Voicevox.SpeakerId);
        _voicevoxClient.OnReady += (() => {
            AddLogMessage(ChatMessageType.LogInfo, "Voicevox is ready.");
            return Task.CompletedTask;
        });
        
        Task.Run(() =>_voicevoxClient.RunAsync());
    }

    private async Task VoicevoxCommand(string arg)
    {
        var strs = arg.Split(' ');
        if (strs[0] == "start" && strs.Length == 1)
        {
            SetVoicevoxClient();
            AddLogMessage(ChatMessageType.LogInfo, "Start Voicevox connections.");
            return;
        }
        await Task.CompletedTask;
    }
}