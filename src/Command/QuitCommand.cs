using Medoz.KoeKan.Data;
using Medoz.KoeKan.Services;

using Microsoft.Extensions.Logging;

namespace Medoz.KoeKan.Command;

public class QuitCommand : ICommand
{
    public string CommandName => "quit";

    public string HelpText => "quit command - extit application.";

    private readonly IWindowService _windowService;
    private readonly ILogger _logger;
    public QuitCommand(
        IWindowService windowService,
        ILogger logger)
    {
        _windowService = windowService;
        _logger = logger;
    }

    public bool CanExecute(string[] args)
    {
        return args.Length == 0 || args[0] == string.Empty;
    }

    public Task ExecuteCommandAsync(string[] args)
    {
        if (!CanExecute(args))
        {
            _logger.LogError("Invalid arguments for quit command.");
            return Task.CompletedTask;
        }

        _windowService.CloseMainWindow();
        return Task.CompletedTask;
    }
}


