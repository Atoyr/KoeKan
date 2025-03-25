
using System.CodeDom;
using System.Runtime.InteropServices;
using System.Windows.Input;

using Medoz.KoeKan.Clients;
using Medoz.KoeKan.Data;
namespace Medoz.KoeKan.Command;

public class RootCommand : ICommand
{
    private readonly string _helpCommandName = "help";
    private readonly Dictionary<string, ICommand> _childCommands = new()
    {
        { "voicevox", new VoicevoxCommand() },
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

        if (args.Args.Length > 0 && !_childCommands.ContainsKey(args.Args[0]))
        {
            args.Listener.AddLogMessage(ChatMessageType.LogInfo, $"command {args.Args[0]} is not found.");
            return;
        }

        if (_childCommands.ContainsKey(_helpCommandName))
        {
            await _childCommands[_helpCommandName].ExecuteCommandAsync(args);
            return;
        }

        args.Listener.AddLogMessage(ChatMessageType.LogInfo, "unknown command.");
        await Task.CompletedTask;
    }
}

