using System;
using System.Reflection;
using Medoz.KoeKan.Services;

namespace Medoz.KoeKan.Command;

/// <summary>
/// ICommandを継承したクラスにサービスを注入してコマンドを作成するファクトリクラス
/// </summary>
public class CommandFactory
{
    private readonly IConfigService _configService;
    private readonly IClientService _clientService;
    private readonly IListenerService _listenerService;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="configService">設定サービス</param>
    /// <param name="clientService">クライアントサービス</param>
    /// <param name="listenerService">リスナーサービス</param>
    public CommandFactory(
        IConfigService configService,
        IClientService clientService,
        IListenerService listenerService)
    {
        _configService = configService;
        _clientService = clientService;
        _listenerService = listenerService;
    }

    /// <summary>
    /// 指定された型のコマンドを作成します
    /// </summary>
    /// <typeparam name="T">作成するコマンドの型（ICommandを実装している必要があります）</typeparam>
    /// <returns>作成されたコマンドインスタンス</returns>
    public T CreateCommand<T>() where T : ICommand
    {
        return (T)CreateCommand(typeof(T));
    }

    /// <summary>
    /// 指定された型のコマンドを作成します
    /// </summary>
    /// <param name="commandType">作成するコマンドの型（ICommandを実装している必要があります）</param>
    /// <returns>作成されたコマンドインスタンス</returns>
    public ICommand CreateCommand(Type commandType)
    {
        if (!typeof(ICommand).IsAssignableFrom(commandType))
        {
            throw new ArgumentException($"Type {commandType.Name} does not implement ICommand interface.");
        }

        // コンストラクタを取得
        var constructors = commandType.GetConstructors();
        if (constructors.Length == 0)
        {
            throw new ArgumentException($"Type {commandType.Name} does not have any public constructors.");
        }

        // 最も多くのパラメータを持つコンストラクタを選択
        var constructor = constructors.OrderByDescending(c => c.GetParameters().Length).First();
        var parameters = constructor.GetParameters();
        var arguments = new object[parameters.Length];

        // コンストラクタの各パラメータに対応するサービスを注入
        for (int i = 0; i < parameters.Length; i++)
        {
            var parameterType = parameters[i].ParameterType;

            if (parameterType == typeof(IConfigService))
            {
                arguments[i] = _configService;
            }
            else if (parameterType == typeof(IClientService))
            {
                arguments[i] = _clientService;
            }
            else if (parameterType == typeof(IListenerService))
            {
                arguments[i] = _listenerService;
            }
            else
            {
                throw new ArgumentException($"Cannot resolve parameter of type {parameterType.Name} for constructor of {commandType.Name}.");
            }
        }

        // コマンドインスタンスを作成して返す
        return (ICommand)constructor.Invoke(arguments);
    }

    /// <summary>
    /// サブコマンドハンドラーを作成します
    /// </summary>
    /// <param name="baseCommand">ベースとなるコマンド</param>
    /// <param name="subCommandTypes">サブコマンドの型の配列</param>
    /// <returns>作成されたサブコマンドハンドラー</returns>
    public SubCommandHandler CreateSubCommandHandler(ICommand baseCommand, params Type[] subCommandTypes)
    {
        var handler = new SubCommandHandler(baseCommand);

        foreach (var subCommandType in subCommandTypes)
        {
            var subCommand = CreateCommand(subCommandType);
            handler.RegisterSubCommand(subCommand);
        }

        return handler;
    }

    /// <summary>
    /// 指定された型のコマンドとそのサブコマンドを一度に作成します
    /// </summary>
    /// <typeparam name="T">作成するコマンドの型（ICommandを実装している必要があります）</typeparam>
    /// <param name="subCommandTypes">サブコマンドの型の配列</param>
    /// <returns>作成されたサブコマンドハンドラー（サブコマンドが指定されていない場合は通常のコマンド）</returns>
    public ICommand CreateCommandWithSubCommands<T>(params Type[] subCommandTypes) where T : ICommand
    {
        return CreateCommandWithSubCommands(typeof(T), subCommandTypes);
    }

    /// <summary>
    /// 指定された型のコマンドとそのサブコマンドを一度に作成します
    /// </summary>
    /// <param name="commandType">作成するコマンドの型（ICommandを実装している必要があります）</param>
    /// <param name="subCommandTypes">サブコマンドの型の配列</param>
    /// <returns>作成されたサブコマンドハンドラー（サブコマンドが指定されていない場合は通常のコマンド）</returns>
    public ICommand CreateCommandWithSubCommands(Type commandType, params Type[] subCommandTypes)
    {
        var baseCommand = CreateCommand(commandType);

        if (subCommandTypes == null || subCommandTypes.Length == 0)
        {
            return baseCommand;
        }

        return CreateSubCommandHandler(baseCommand, subCommandTypes);
    }

    /// <summary>
    /// コマンドマネージャーを初期化します
    /// </summary>
    /// <param name="commandManager">初期化するコマンドマネージャー</param>
    public void InitializeCommandManager(CommandManager commandManager)
    {
        // Discordコマンド
        commandManager.RegisterCommand(
            CreateCommandWithSubCommands<DiscordCommand>(
                typeof(DiscordCommand_Init),
                typeof(DiscordCommand_Start),
                typeof(DiscordCommand_Guilds),
                typeof(DiscordCommand_Channels),
                typeof(DiscordCommand_Set)
            )
        );

        // Twitchコマンド
        commandManager.RegisterCommand(
            CreateCommandWithSubCommands<TwitchCommand>(
                typeof(TwitchCommand_Start)
            )
        );

        // Voicevoxコマンド
        commandManager.RegisterCommand(
            CreateCommandWithSubCommands<VoicevoxCommand>(
                typeof(VoicevoxCommand_Init),
                typeof(VoicevoxComand_Start)
            )
        );

        // その他のコマンド
        commandManager.RegisterCommand(CreateCommand<ClearCommand>());
        commandManager.RegisterCommand(CreateCommand<SetCommand>());
        commandManager.RegisterCommand(CreateCommand<WriteCommand>());
    }
}