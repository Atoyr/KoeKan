using System.CodeDom;
using System.Runtime.InteropServices;
using System.Windows.Input;

using Medoz.KoeKan.Clients;
using Medoz.KoeKan.Services;
using Medoz.KoeKan.Data;
namespace Medoz.KoeKan.Command;

public class SetCommand : ICommand
{
    public string CommandName => "set";
    public string HelpText => "set config property";

    private readonly IListenerService _listenerService;
    private readonly IConfigService _configService;

    private readonly IWindowService _windowService;

    public SetCommand(IListenerService listenerService, IConfigService configService, IWindowService windowService)
    {
        _windowService = windowService;
        _listenerService = listenerService;
        _configService = configService;
    }

    public bool CanExecute(string[] args)
    {
        return args.Length >= 0;
    }

    public async Task ExecuteCommandAsync(string[] args)
    {
        if (!CanExecute(args))
        {
            _listenerService.AddLogMessage("Invalid arguments for set command.");
            return;
        }

        if (args.Length == 0)
        {
            _windowService.OpenSettingWindow();
            return;
        }

        var config = _configService.GetConfig();
        switch (args[0])
        {
            case "username":
                if (args.Length != 2)
                {
                    _listenerService.AddLogMessage("Username not specified.");
                    return;
                }
                config.Username = args[1];
                _configService.SaveConfig();
                _listenerService.AddLogMessage($"Username set to {args[1]}.");
                break;
            case "icon":
                if (args.Length != 2)
                {
                    _listenerService.AddLogMessage("icon not specified.");
                    return;
                }
                config.Icon = args[1];
                _configService.SaveConfig();
                _listenerService.AddLogMessage($"Icon set to {args[1]}.");
                break;
            case "application":
                if (args.Length < 2)
                {
                    _listenerService.AddLogMessage("application args not found.");
                    return;
                }
                config.Applications.Concat(args[1..]);
                _configService.SaveConfig();
                _listenerService.AddLogMessage($"append application to {args[1..]}.");
                break;
            default:
                // FIXME: 設定画面を開く
                _listenerService.AddLogMessage($"Unknown set command: {args[0]}");
                break;
        }

        await Task.CompletedTask;
    }
}
