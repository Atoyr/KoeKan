using Medoz.KoeKan.Services;

using Microsoft.Extensions.Logging;

namespace Medoz.KoeKan.Command;

public class WindowCommand_Move : ICommand
{
    public string CommandName => "move";
    public string HelpText => "ウィンドウを動かせる状態にします。使用法: window move [on|off]";

    private readonly IWindowService _windowService;
    private readonly ILogger _logger;

    public WindowCommand_Move(
        IWindowService windowService,
        ILogger logger)
    {
        _windowService = windowService;
        _logger = logger;
    }

    public bool CanExecute(string[] args)
    {
        // 引数なし、または引数が1つ（on/off）の場合に実行可能
        return args.Length == 0 ||
               (args.Length == 1 && (args[0].ToLower() == "on" || args[0].ToLower() == "off"));
    }

    public async Task ExecuteCommandAsync(string[] args)
    {
        if (args.Length == 0)
        {
            // 引数なしの場合は現在の状態を切り替える
            _windowService.ToggleMoveableWindow();
            _logger.LogInformation("Window moveable state toggled.");
        }
        else if (args.Length == 1)
        {
            // on/offが指定された場合
            bool isMoveable = args[0].ToLower() == "on";
            _windowService.ChangeMoveableWindowState(isMoveable);
            _logger.LogInformation($"Window moveable state set to {isMoveable}.");
        }

        await Task.CompletedTask;
    }
}