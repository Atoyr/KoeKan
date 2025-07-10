using System.CodeDom;
using System.Runtime.InteropServices;
using System.Windows.Input;

using Medoz.KoeKan.Services;
namespace Medoz.KoeKan.Command;

public class ClearCommand : ICommand
{
    public string CommandName => "clear";
    public string HelpText => "clear command is clearing the listner text.";

    private readonly IListenerService _listenerService;

    public ClearCommand(IListenerService listenerService)
    {
        _listenerService = listenerService;
    }

    public bool CanExecute(string[] args)
    {
        return args.Length == 0;
    }

    public async Task ExecuteCommandAsync(string[] args)
    {
        _listenerService.Clear();
        await Task.CompletedTask;
    }
}

