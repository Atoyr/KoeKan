using System.CodeDom;
using System.Runtime.InteropServices;
using System.Windows.Input;

using Medoz.KoeKan.Services;
namespace Medoz.KoeKan.Command;

public class ClearCommand : ICommand
{
    public string CommandName => "clear";
    public string HelpText => "clear command is clearing the listner text.";

    private readonly IWindowService _windowService;

    public ClearCommand(IWindowService windowService)
    {
        _windowService = windowService;
    }

    public bool CanExecute(string[] args)
    {
        return args.Length == 0;
    }

    public async Task ExecuteCommandAsync(string[] args)
    {
        _windowService.MainWindowMessageClear();
        await Task.CompletedTask;
    }
}
