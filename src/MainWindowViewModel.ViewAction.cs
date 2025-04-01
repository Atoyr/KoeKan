using System.Text;
using System.IO;
using System.Windows.Threading;

using Medoz.KoeKan.Clients;
using Medoz.KoeKan.Data;
using Medoz.KoeKan.Command;
using System.Windows;

namespace Medoz.KoeKan;

public partial class MainWindowViewModel
{

    // View Action
    public Action? CloseAction { get; set; }
    public Action? ToggleMoveableWindowAction { get; set; }
    public Action<double, double>? SetWindowSizeAction { get; set; }
    public Dispatcher? Dispatcher { get; set; }
    public Action? OpenSettingWindow { get; set; }

    class QCommand: ICommand
    {
        private readonly MainWindowViewModel _viewModel;
        public QCommand(MainWindowViewModel viewModel)
        {
            _viewModel = viewModel;
        }
        public Task ExecuteCommandAsync(CommandArgs args)
        {
            var commandArgs = args.Args.Skip(1).ToArray();
            _viewModel.QuitCommand(commandArgs);

            // TODO: 引数がなかったらエラーとする
            return Task.CompletedTask;
        }
    }

    class WindowCommand : ICommand
    {
        private readonly Dictionary<string, Action<string[]>> _commands = new()
        {
            { "quit", (args) => { } },
            { "setsize", (args) => { } },
        };

        private readonly MainWindowViewModel _viewModel;
        public WindowCommand(MainWindowViewModel viewModel)
        {
            _viewModel = viewModel;
            _commands["move"] = _viewModel.ToggleMoveableWindow;
            _commands["size"] = _viewModel.SetWindowSize;
            _commands["quit"] = _viewModel.QuitCommand;
        }
        public Task ExecuteCommandAsync(CommandArgs args)
        {
            var commandName = args.Args[0];
            var commandArgs = args.Args.Skip(1).ToArray();
            if (_commands.ContainsKey(commandName))
            {
                _commands[commandName](commandArgs);
                return Task.CompletedTask;
            }

            // TODO: 引数がなかったらエラーとする
            return Task.CompletedTask;
        }
    }

    private void QuitCommand(string[] args)
    {
        if (args.Length == 0)
        {
            CloseAction?.Invoke();
            return;
        }
        // TODO: 引数があったらエラーとする
    }

    private void ToggleMoveableWindow(string[] args)
    {
        if (args.Length == 0)
        {
            ToggleMoveableWindowAction?.Invoke();
            return;
        }
        // TODO: 引数があったらエラーとする
    }

    private void SetWindowSize(string[] args)
    {
        if (args.Length == 2
        && int.TryParse(args[0], out int w)
        && int.TryParse(args[1], out int h))
        {
            _config.Width = w;
            _config.Height = h;
            SetWindowSizeAction?.Invoke(w, h);
            _config.Save();
            Listener?.AddLogMessage(ChatMessageType.LogSuccess, $"Change Window Size. Widht:{w} Height:{h}");
            return;
        }

        // TODO: 引数がなかったらエラーとする
    }

}