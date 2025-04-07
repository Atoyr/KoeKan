using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

using Medoz.KoeKan.Clients;
using Medoz.KoeKan.Data;
using Medoz.KoeKan.Command;

namespace Medoz.KoeKan;

public partial class MainWindowViewModel
{

    // View Action
    public Dispatcher? Dispatcher { get; set; }
    public Action? OpenSettingWindow { get; set; }

    class QCommand : ICommand
    {
        private readonly MainWindowViewModel _viewModel;

        public string CommandName => "q";
        public string HelpText => "アプリケーションを終了します";

        public QCommand(MainWindowViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public bool CanExecute(string[] args)
        {
            return true;
        }

        public Task ExecuteCommandAsync(string[] args)
        {
            _viewModel.QuitCommand(args);
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

        public string CommandName => "window";
        public string HelpText => "ウィンドウ操作を行います";

        private readonly MainWindowViewModel _viewModel;

        public WindowCommand(MainWindowViewModel viewModel)
        {
            _viewModel = viewModel;
            _commands["move"] = _viewModel.ToggleMoveableWindow;
            _commands["size"] = _viewModel.SetWindowSize;
            _commands["quit"] = _viewModel.QuitCommand;
        }

        public bool CanExecute(string[] args)
        {
            if (args.Length == 0) return false;
            return _commands.ContainsKey(args[0]);
        }

        public Task ExecuteCommandAsync(string[] args)
        {
            if (args.Length == 0) return Task.CompletedTask;

            var commandName = args[0];
            var commandArgs = args.Skip(1).ToArray();

            if (_commands.ContainsKey(commandName))
            {
                _commands[commandName](commandArgs);
            }

            return Task.CompletedTask;
        }
    }

    private void QuitCommand(string[] args)
    {
        if (args.Length == 0)
        {
            _windowService.Close();
            return;
        }
        // TODO: 引数があったらエラーとする
    }
    private void ToggleMoveableWindow(string[] args)
    {
        if (args.Length == 0)
        {
            _windowService.ToggleMoveableWindow();
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
            _configService.GetConfig().Width = w;
            _configService.GetConfig().Height = h;
            _windowService.SetWindowSize(w, h);
            _configService.SaveConfig();
            _listenerService.GetListener()?.AddLogMessage(ChatMessageType.LogSuccess, $"Change Window Size. Widht:{w} Height:{h}");
            return;
        }

        // TODO: 引数がなかったらエラーとする
    }

}