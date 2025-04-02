using System.CodeDom;
using System.Runtime.InteropServices;
using System.Windows.Input;

using Medoz.KoeKan.Clients;
using Medoz.KoeKan.Data;
namespace Medoz.KoeKan.Command;

public class SetCommand : ICommand
{
    public string CommandName => "set";
    public string HelpText => "set command";

    public bool CanExecute(string[] args)
    {
        return args.Length > 0 && args[0] == CommandName;
    }

    public async Task ExecuteCommandAsync(string[] args)
    {
        await Task.CompletedTask;
    }
}
