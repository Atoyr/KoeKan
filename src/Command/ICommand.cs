namespace Medoz.KoeKan.Command;

public interface ICommand
{
    string CommandName { get; }
    string HelpText { get; }
    bool CanExecute(string[] args);
    Task ExecuteCommandAsync(string[] args);
}