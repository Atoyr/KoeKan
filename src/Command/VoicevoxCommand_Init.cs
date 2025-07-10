using System.CodeDom;
using System.Runtime.InteropServices;
using System.Windows.Input;

using Medoz.KoeKan.Clients;
using Medoz.KoeKan.Data;
using Medoz.KoeKan.Services;

using Microsoft.Extensions.Options;
namespace Medoz.KoeKan.Command;

public class VoicevoxCommand_Init : ICommand
{
    public string CommandName => "init";
    public string HelpText => "[textClientName?] init voicevox client setting.";

    private readonly IListenerService _listenerService;
    private readonly IConfigService _configService;

    public VoicevoxCommand_Init(
        IListenerService listenerService,
        IClientService clientService,
        IConfigService configService)
    {
        _listenerService = listenerService;
        _configService = configService;
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
        var clientConfigName = GetClientConfigName(clientName);
        config.Clients.TryGetValue(clientConfigName, out DynamicConfig? clientConfig);
        if (clientConfig is null)
        {
            clientConfig = new DynamicConfig();
            config.Clients[clientConfigName] = clientConfig;
        }

        if (clientConfig.TryGetValue("speaker_id", out uint speakerId) == false)
        {
            clientConfig["speaker_id"] = 0;
        }
        if (clientConfig.TryGetValue("url", out string? url) == false)
        {
            clientConfig["url"] = "http://localhost:50021";
        }
        _configService.SaveConfig();
        _listenerService.AddLogMessage($"Initialze Voicevox {clientName}");
        await Task.CompletedTask;
    }

    private string GetClientConfigName(string? clientName)
    {
        if (string.IsNullOrEmpty(clientName))
        {
            return "voicevox";
        }
        return $"voicevox_{clientName}";
    }
}
