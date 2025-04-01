namespace Medoz.KoeKan.Command;

public class CommandDelegate : ICommand
{

    private readonly Action<CommandArgs> _action;

    private CommandDelegate()
    {
        _action = (args) => { };
    }

    public CommandDelegate(Action<CommandArgs> action)
    {
        _action = action;
    }

    public async Task ExecuteCommandAsync(CommandArgs args)
    {
        _action(args);
        await Task.CompletedTask;
    }
}