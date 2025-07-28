namespace Medoz.KoeKan.Command;

public class TwitchCommand : ICommand
{
    public string CommandName => "twitch";

    public string HelpText => "twitch command";

    public bool CanExecute(string[] args)
    {
        return false;
    }

    public async Task ExecuteCommandAsync(string[] args)
    {
        await Task.CompletedTask;
    }
}