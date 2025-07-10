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
        return args.Length == 0 || args[0] == string.Empty;
    }

    public async Task ExecuteCommandAsync(string[] args)
    {
        if (!CanExecute(args))
        {
            _listenerService.AddLogMessage("Invalid arguments for write command.");
            return;
        }

        try
        {
            _configService.Save();
            _listenerService.AddLogMessage("Config saved.");
        }
        catch (Exception)
        {
            _listenerService.AddLogMessage("Save config is faild.");
        }
        await Task.CompletedTask;
    }
}

