using Medoz.KoeKan.Services;

using Microsoft.Extensions.Logging;

namespace Medoz.KoeKan.Command;

public class WindowCommand_Size : ICommand
{
    public string CommandName => "size";
    public string HelpText => "ウィンドウのサイズを変更します。使用法: window size [width] [height]";

    private readonly IWindowService _windowService;
    private readonly ILogger _logger;

    public WindowCommand_Size(
        IWindowService windowService,
        ILogger logger)
    {
        _windowService = windowService;
        _logger = logger;
    }

    public bool CanExecute(string[] args)
    {
        // 引数が2つ（幅と高さ）あり、両方が数値に変換できる場合に実行可能
        return args.Length == 2 &&
               double.TryParse(args[0], out _) &&
               double.TryParse(args[1], out _);
    }

    public async Task ExecuteCommandAsync(string[] args)
    {
        if (double.TryParse(args[0], out double width) &&
            double.TryParse(args[1], out double height))
        {
            _windowService.SetMainWindowSize(width, height);
            _logger.LogInformation($"Window size changed: Width={width}, Height={height}");
        }
        else
        {
            _logger.LogError("Invalid size specified. Please enter numeric values for width and height.");
        }

        await Task.CompletedTask;
    }
}