using System.CodeDom;
using System.Runtime.InteropServices;
using System.Windows.Input;

using Medoz.KoeKan.Clients;
using Medoz.KoeKan.Data;
namespace Medoz.KoeKan.Command;

public class VoicevoxCommand : ICommand
{
    private readonly Dictionary<string, ICommand> _childCommands = new()
    {
        { "start", new VoicevoxCommand_Start() },
    };

    public async Task ExecuteCommandAsync(CommandArgs args)
    {

        if (args.Args.Length > 0 && _childCommands.ContainsKey(args.Args[0]))
        {
            var command = _childCommands[args.Args[0]];
            args.Next();
            await command.ExecuteCommandAsync(args);
            return;
        }

        HelpCommand(args);
        await Task.CompletedTask;
    }

    private void HelpCommand(CommandArgs args)
    {
    }
}