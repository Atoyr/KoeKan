namespace Medoz.KoeKan.Command;

public class SubCommandHandler : ICommand
{
    protected ICommand _command;
    protected readonly Dictionary<string, ICommand> _subCommands = new();
    protected string _helpText;
    public string CommandName => _command.CommandName;
    public string HelpText => _helpText;

    public SubCommandHandler(ICommand command)
    {
        _command = command;
        _helpText = command.HelpText;
    }

    public void RegisterSubCommand(ICommand subCommand)
    {
        if (_subCommands.ContainsKey(subCommand.CommandName))
        {
            throw new ArgumentException($"Command {subCommand.CommandName} is already registered.");
        }

        _subCommands[subCommand.CommandName] = subCommand;
        UpdateHelpText();
    }

    protected void UpdateHelpText()
    {
        // ベースヘルプテキスト
        _helpText = _command.HelpText;

        if (_subCommands.Count == 0)
        {
            return;
        }

        _helpText += $"\n:{_command.CommandName} [subcommand] - Execute {_command.CommandName} with one of these subcommands:";

        // サブコマンドのヘルプを追加
        foreach (var subCommand in _subCommands.Values)
        {
            _helpText += $"\n  - {subCommand.CommandName}: {subCommand.HelpText}";
        }
    }

    protected bool CanExecuteSubCommand(string[] args)
    {
        if (args.Length == 0)
        {
            return false; // 引数がない場合は実行できない
        }

        var subCommandName = args[0];
        if (_subCommands.TryGetValue(subCommandName, out ICommand? subCommand))
        {
            // サブコマンド名を除いた残りの引数で検証
            string[] subArgs = args.Skip(1).ToArray();
            return subCommand.CanExecute(subArgs);
        }

        return false;
    }

    protected async Task ExecuteSubCommandAsync(string[] args)
    {
        if (args.Length > 0 && _subCommands.ContainsKey(args[0]))
        {
            var subCommand = _subCommands[args[0]];
            var subArgs = args.Skip(1).ToArray();
            await subCommand.ExecuteCommandAsync(args);
        }
    }

    public bool CanExecute(string[] args)
    {
        return _command.CanExecute(args) || CanExecuteSubCommand(args);
    }

    public async Task ExecuteCommandAsync(string[] args)
    {
        if (_command.CanExecute(args))
        {
            await _command.ExecuteCommandAsync(args);
            return;
        }

        if (CanExecuteSubCommand(args))
        {
            await ExecuteSubCommandAsync(args);
            return;
        }

        // ヘルプを表示するか、エラーを返す
        await Task.CompletedTask;
    }
}