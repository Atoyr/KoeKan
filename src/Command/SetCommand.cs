using System.CodeDom;
using System.Runtime.InteropServices;
using System.Windows.Input;

using Medoz.KoeKan.Clients;
using Medoz.KoeKan.Services;
using Medoz.KoeKan.Data;
using Microsoft.Extensions.Logging;
namespace Medoz.KoeKan.Command;

public class SetCommand : ICommand
{
    public string CommandName => "set";
    public string HelpText => "set config property";

    private readonly IConfigService _configService;

    private readonly IWindowService _windowService;

    private readonly ILogger _logger;

    public SetCommand(
        IConfigService configService,
        IWindowService windowService,
        ILogger logger)
    {
        _windowService = windowService;
        _configService = configService;
        _logger = logger;
    }

    public bool CanExecute(string[] args)
    {
        return args.Length >= 0;
    }

    public async Task ExecuteCommandAsync(string[] args)
    {
        if (!CanExecute(args))
        {
            _logger.LogError("Invalid arguments for set command.");
            return;
        }

        if (args.Length == 0)
        {
            _windowService.OpenSettingsWindow();
            return;
        }

        var config = _configService.GetConfig();
        switch (args[0])
        {
            case "username":
                if (args.Length != 2)
                {
                    _logger.LogError("Username not specified.");
                    return;
                }
                config.Username = args[1];
                _configService.SaveConfig();
                _logger.LogInformation($"Username set to {args[1]}.");
                break;
            case "icon":
                if (args.Length != 2)
                {
                    _logger.LogError("Icon not specified.");
                    return;
                }
                config.Icon = args[1];
                _configService.SaveConfig();
                _logger.LogInformation($"Icon set to {args[1]}.");
                break;
            case "application":
                if (args.Length < 2)
                {
                    _logger.LogError("Application args not found.");
                    return;
                }
                config.Applications.Concat(args[1..]);
                _configService.SaveConfig();
                _logger.LogInformation($"Application args set to {string.Join(" ", args[1..])}.");
                break;
            default:
                _windowService.OpenSettingsWindow();
                break;
        }

        await Task.CompletedTask;
    }
}
