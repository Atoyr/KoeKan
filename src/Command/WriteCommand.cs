using Medoz.KoeKan.Data;
using Medoz.KoeKan.Services;

using Microsoft.Extensions.Logging;

namespace Medoz.KoeKan.Command;

public class WriteCommand : ICommand
{
    public string CommandName => "write";

    public string HelpText => "write command - save config file.";

    private readonly IConfigService _configService;
    private readonly ILogger _logger;

    public WriteCommand(
        IConfigService configService,
        ILogger logger)
    {
        _configService = configService;
        _logger = logger;
    }

    public bool CanExecute(string[] args)
    {
        return args.Length == 0 || args[0] == string.Empty;
    }

    public async Task ExecuteCommandAsync(string[] args)
    {
        if (!CanExecute(args))
        {
            _logger.LogError("Invalid arguments for write command.");
            return;
        }

        try
        {
            _configService.Save();
            _logger.LogInformation("Config saved successfully.");
        }
        catch (Exception)
        {
            _logger.LogError("Failed to save config.");
        }
        await Task.CompletedTask;
    }
}

