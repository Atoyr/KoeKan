using System.CodeDom;
using System.Runtime.InteropServices;
using System.Windows.Input;

using Medoz.KoeKan.Clients;
using Medoz.KoeKan.Data;
namespace Medoz.KoeKan.Command;

public class VoicevoxCommand_Start : ICommand
{
    public async Task ExecuteCommandAsync(CommandArgs args)
    {
        var clientName = "default";
        if (args.Options.ContainsKey("client"))
        {
            clientName = args.Options["client"];
        }
        var client = args.Clients[clientName];
        var speaker = new VoicevoxClient(new VoicevoxOptions() { SpeakerId = args.Config.Voicevox.SpeakerId });
        speaker.OnReady += (() => {
            args.Listener.AddLogMessage(ChatMessageType.LogInfo, "Voicevox is ready.");
            return Task.CompletedTask;
        });
        args.Clients[clientName] = new TextToSpeechBridge(client, speaker);

        var bridge = args.Clients[clientName];
        var _ = bridge.RunAsync();
        args.Listener.AddLogMessage(ChatMessageType.LogInfo, "Start Voicevox connections.");
        await Task.CompletedTask;
    }
}
