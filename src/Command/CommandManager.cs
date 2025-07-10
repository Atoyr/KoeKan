namespace Medoz.KoeKan.Command;

public class CommandManager
{
    private readonly Dictionary<string, ICommand> _commands = new();

    /// <summary>
    /// コマンドを登録します。
    /// </summary>
    /// <param name="command"></param>
    /// <param name="alias"></param>
    public void RegisterCommand(ICommand command, string? alias = null)
    {
        if (!_commands.ContainsKey(command.CommandName))
        {
            _commands.Add(command.CommandName, command);
        }
        if (!string.IsNullOrWhiteSpace(alias) && !_commands.ContainsKey(alias))
        {
            _commands.Add(alias, command);
        }
    }

    /// <summary>
    /// コマンドを実行します。
    /// </summary>
    /// <param name="commandLine">コマンド</param>
    /// <returns>コマンドが存在しない場合はfalse</returns>
    public async Task<bool> TryExecuteCommandAsync(string commandLine)
    {
        if (string.IsNullOrWhiteSpace(commandLine))
        {
            return false;
        }

        // コマンド名と引数を分離
        string[] parts = commandLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            return false;
        }

        string commandName = parts[0];
        string[] args = parts.Skip(1).ToArray();

        // コマンドを検索して実行
        if (_commands.TryGetValue(commandName, out ICommand? command))
        {
            if (command.CanExecute(args))
            {
                await command.ExecuteCommandAsync(args);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// コマンドの一覧を取得します。
    /// </summary>
    /// <returns></returns>
    public string[] GetAvailableCommands()
    {
        return _commands.Keys.ToArray();
    }

    /// <summary>
    /// コマンドのヘルプテキストを取得します。
    /// </summary>
    /// <param name="commandName"></param>
    /// <returns></returns>
    public string GetHelpText(string? commandName)
    {
        if (_commands.TryGetValue(commandName ?? "", out ICommand? command))
        {
            return command.HelpText;
        }

        return $"Command '{commandName ?? ""}' not found.";
    }
}