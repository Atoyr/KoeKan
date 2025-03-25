namespace Medoz.KoeKan.Command;

public interface ICommand
{
    Task ExecuteCommandAsync(CommandArgs args);
}