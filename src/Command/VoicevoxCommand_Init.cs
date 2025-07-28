using Medoz.KoeKan.Data;
using Medoz.KoeKan.Services;

using Microsoft.Extensions.Logging;
namespace Medoz.KoeKan.Command;

public class VoicevoxCommand_Init : ICommand
{
    public string CommandName => "init";
    public string HelpText => "[textClientName?] init voicevox client setting.";

    private readonly IConfigService _configService;
    private readonly ILogger _logger;

    public VoicevoxCommand_Init(
        IConfigService configService,
        ILogger logger)
    {
        _configService = configService;
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
        _logger.LogInformation($"Initialized Voicevox client with name: {clientName ?? "default"}");
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
