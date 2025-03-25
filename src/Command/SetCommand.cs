using System.CodeDom;
using System.Runtime.InteropServices;
using System.Windows.Input;

using Medoz.KoeKan.Clients;
using Medoz.KoeKan.Data;
namespace Medoz.KoeKan.Command;

public class SetCommand : ICommand
{
    private readonly Dictionary<string, ICommand> _commands = new()
    {
        { "start", new VoicevoxCommand_Start() },
    };

    public async Task ExecuteCommandAsync(CommandArgs args)
    {
        if (args.Args.Length == 0 || args.Args[0] == string.Empty)
        {
            return;
        }

        if (args.Args[0] == "start" && args.Args.Length == 1)
        {
            if (_commands.ContainsKey("start"))
            {
                _ = _commands["start"].ExecuteCommandAsync(args);
            }
        }
        await Task.CompletedTask;
    }
}
