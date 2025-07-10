using System.CodeDom;
using System.Runtime.InteropServices;
using System.Windows.Input;

using Medoz.KoeKan.Clients;
using Medoz.KoeKan.Data;
namespace Medoz.KoeKan.Command;

public class WebApiCommand : ICommand
{
    public string CommandName => "webapi";

    public string HelpText => "webapi command";

    public bool CanExecute(string[] args)
    {
        return false;
    }

    public async Task ExecuteCommandAsync(string[] args)
    {
        await Task.CompletedTask;
    }
}
