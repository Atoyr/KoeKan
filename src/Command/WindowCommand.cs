using System.Windows.Input;
using Medoz.KoeKan.Services;

namespace Medoz.KoeKan.Command;

public class WindowCommand : ICommand
{
    public string CommandName => "window";

    public string HelpText => "ウィンドウ操作コマンド";

    public bool CanExecute(string[] args)
    {
        return false;
    }

    public async Task ExecuteCommandAsync(string[] args)
    {
        await Task.CompletedTask;
    }
}