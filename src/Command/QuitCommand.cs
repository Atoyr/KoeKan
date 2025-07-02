using Medoz.KoeKan.Data;
using Medoz.KoeKan.Services;

namespace Medoz.KoeKan.Command;

public class QuitCommand : ICommand
{
    public string CommandName => "quit";

    public string HelpText => "quit command - extit application.";

    private readonly IWindowService _windowService;
    private readonly IListenerService _listenerService;

    public QuitCommand(
        IListenerService listenerService,
        IWindowService windowService)
    {
        _listenerService = listenerService;
        _windowService = windowService;
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

        _windowService.CloseMainWindow();
        await Task.CompletedTask;
    }
}


