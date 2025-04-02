using Medoz.KoeKan.Data;
using Medoz.KoeKan.Services;

namespace Medoz.KoeKan.Command;

public class WriteCommand : ICommand
{
    public string CommandName => "write";

    public string HelpText => "write command - save config file.";

    private readonly IConfigService _configService;
    private readonly IListenerService _listenerService;

    public WriteCommand(
        IListenerService listenerService,
        IConfigService configService)
    {
        _listenerService = listenerService;
        _configService = configService;
    }

    public bool CanExecute(string[] args)
    {
        if (args.Length == 0 || args[0] == string.Empty)
        {
            return true;
        }
        return false;
    }

    public async Task ExecuteCommandAsync(string[] args)
    {
        if (args.Length == 0 || args[0] == string.Empty)
        {
            try
            {
                _configService.SaveConfig();
                _listenerService.AddLogMessage("Config saved.");
            }
            catch (System.Exception)
            {
                _listenerService.AddLogMessage("Save config is faild.");
            }
        }
        await Task.CompletedTask;
    }
}

