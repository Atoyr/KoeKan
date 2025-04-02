using System.CodeDom;
using System.Runtime.InteropServices;
using System.Windows.Input;

using Medoz.KoeKan.Clients;
using Medoz.KoeKan.Data;
namespace Medoz.KoeKan.Command;

public class DiscordCommand : ICommand
{
    public string CommandName => "discord";

    public string HelpText => "discord command";

    public bool CanExecute(string[] args)
    {
        return false;
    }

    public async Task ExecuteCommandAsync(string[] args)
    {
        await Task.CompletedTask;
    }
}
