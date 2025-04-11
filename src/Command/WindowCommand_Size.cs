using Medoz.KoeKan.Services;

namespace Medoz.KoeKan.Command;

public class WindowCommand_Size : ICommand
{
    public string CommandName => "size";
    public string HelpText => "ウィンドウのサイズを変更します。使用法: window size [width] [height]";

    private readonly IWindowService _windowService;
    private readonly IListenerService _listenerService;

    public WindowCommand_Size(
        IWindowService windowService,
        IListenerService listenerService)
    {
        _windowService = windowService;
        _listenerService = listenerService;
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
            _windowService.SetWindowSize(width, height);
            _listenerService.AddLogMessage($"ウィンドウサイズを変更しました: 幅={width}, 高さ={height}");
        }
        else
        {
            _listenerService.AddLogMessage("無効なサイズが指定されました。数値を入力してください。");
        }

        await Task.CompletedTask;
    }
}