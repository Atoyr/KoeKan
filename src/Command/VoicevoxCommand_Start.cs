using System.CodeDom;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Navigation;

using Medoz.KoeKan.Clients;
using Medoz.KoeKan.Data;
using Medoz.KoeKan.Services;

using Microsoft.Extensions.Logging;
namespace Medoz.KoeKan.Command;

public class VoicevoxCommand_Start : ICommand
{
    public string CommandName => "start";
    public string HelpText => "[textClientName?] start voicevox client for textClient.";

    private readonly IClientService _clientService;
    private readonly IConfigService _configService;
    private readonly ISpeakerService _speakerService;
    private readonly ILogger _logger;


    public VoicevoxCommand_Start(
        IClientService clientService,
        IConfigService configService,
        ISpeakerService speakerService,
        ILogger logger
        )
    {
        _configService = configService;
        _clientService = clientService;
        _speakerService = speakerService;
        _logger = logger;
    }

    public bool CanExecute(string[] args)
    {
        return args.Length <= 1;
    }

    public async Task ExecuteCommandAsync(string[] args)
    {
        string? clientName = null;
        if (args.Length > 0)
        {
            clientName = args[0];
        }

        var config = _configService.GetConfig();
        DynamicConfig? clientConfig;
        config.Clients.TryGetValue("voicevox", out clientConfig);
        if (clientConfig == null)
        {
            _logger.LogError($"Voicevox client {clientName ?? ""} config not found.");
            return;
        }

        if (clientConfig.TryGetValue("speaker_id", out uint speakerId) == false)
        {
            _logger.LogError($"Voicevox client {clientName ?? ""} speaker_id not found.");
            return;
        }
        clientConfig.TryGetValue("url", out string? url);

        _speakerService.GetOrCreateSpeaker<VoicevoxClient>(
            new VoicevoxOptions()
            {
                SpeakerId = speakerId,
                Url = url,
            },
            clientName ?? "_");

        await Task.CompletedTask;
    }
}