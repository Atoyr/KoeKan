using Medoz.KoeKan.Clients;
using Medoz.KoeKan.Data;

namespace Medoz.KoeKan;

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
        else if (strs[0] == "")
        await Task.CompletedTask;
    }

    private void SetVoicevoxSpeakerId(uint speakerId)
    {
        if(_voicevoxClient is not null)
        {
            _voicevoxClient.SetSpeakerId(speakerId);
            AddLogMessage(ChatMessageType.LogInfo, $"Voicevox speaker changed to {speakerId}.");
        }
    }
}