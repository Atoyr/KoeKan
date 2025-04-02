using System.CodeDom;
using System.Runtime.InteropServices;
using System.Windows.Input;

using Medoz.KoeKan.Clients;
using Medoz.KoeKan.Data;
using Medoz.KoeKan.Services;
namespace Medoz.KoeKan.Command;

public class VoicevoxCommand_Start : ICommand
{
    public string CommandName => "start";
    public string HelpText => "[textClientName?] start voicevox client for textClient.";

    private readonly IListenerService _listenerService;
    private readonly IClientService _clientService;
    private readonly IConfigService _configService;

    public VoicevoxCommand_Start(
        IListenerService listenerService,
        IClientService clientService,
        IConfigService configService)
    {
        _listenerService = listenerService;
        _configService = configService;
        _clientService = clientService;
    }

    public bool CanExecute(string[] args)
    {
        return args.Length == 0;
    }

    public async Task ExecuteCommandAsync(string[] args)
    {
        string? clientName = null;
        if (args.Length > 0)
        {
            clientName = args[0];
        }
        // TODO: speakerIdを取得する
        var speaker = new VoicevoxClient(new VoicevoxOptions() { });
        speaker.OnReady += (() => {
            _listenerService.AddLogMessage("Voicevox is ready.");
            return Task.CompletedTask;
        });
        _clientService.AppendSpeaker(speaker, clientName);

        var bridge = _clientService.GetClient(clientName);
        var _ = bridge.RunAsync();
        _listenerService.AddLogMessage("Start Voicevox connections.");
        await Task.CompletedTask;
    }
}