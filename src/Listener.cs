using System.Collections.ObjectModel;
using System.Windows.Threading;

using Microsoft.Extensions.Logging;

using Medoz.KoeKan.Clients;
using Medoz.KoeKan.Data;
using System.Windows.Data;

namespace Medoz.KoeKan;

public class Listener : IDisposable
{
    public ObservableCollection<ChatMessage> Messages { get; init; } = new();

    internal Dispatcher? Dispatcher { get; set; }

    private readonly Dictionary<string, Func<ClientMessage, ChatMessage>> _messageConverter = new();

    private readonly ILogger? _logger;

    private readonly uint _MessageCooldown = 60;

    public void Dispose()
    {
        Messages.Clear();
    }

    public Listener(Dispatcher? dispatcher = null, ILogger? logger = null)
    {
        Dispatcher = dispatcher;
        _logger = logger;

        BindingOperations.EnableCollectionSynchronization(Messages, new object());
    }

    public void AddMessageConverter(string clientType, Func<ClientMessage, ChatMessage> sender)
    {
        _messageConverter[clientType] = sender;
    }

    public void RemoveMessageConverter(string clientType)
    {
        _messageConverter.Remove(clientType);
    }

    public void AddLogMessage(ChatMessageType chatMessageType, string message)
        => AddMessage(new ChatMessage(chatMessageType, "", null, "SYSTEM", message, DateTime.Now, IsConsecutiveMessage(chatMessageType, "", "SYSTEM", DateTime.Now)));

    public void AddCommandMessage(string message)
        => AddMessage(new ChatMessage(ChatMessageType.Command, "", null, "COMMAND", message, DateTime.Now, IsConsecutiveMessage(ChatMessageType.Command, "", "COMMAND", DateTime.Now)));

    public void AddMessage(ChatMessage cm)
    {
        cm = cm with { IsConsecutiveMessage = IsConsecutiveMessage(cm.MessageType, cm.Channel, cm.Username, cm.Timestamp) };
        Dispatch(() => Messages.Add(cm));

        if (_logger is not null)
        {
            switch (cm.MessageType)
            {
                case ChatMessageType.LogInfo:
                    _logger.LogInformation(cm.Message);
                    break;
                case ChatMessageType.LogSuccess:
                    _logger.LogInformation(cm.Message);
                    break;
                case ChatMessageType.LogWarning:
                    _logger.LogWarning(cm.Message);
                    break;
                case ChatMessageType.LogFatal:
                    _logger.LogError(cm.Message);
                    break;
                default:
                    break;
            }
        }
    }

    public void ClearMessage()
    {
        Dispatch(() => Messages.Clear());
    }

    /// <summary>
    /// Dispatcherを使ってUIスレッドで処理を行う
    /// </summary>
    /// <param name="action"></param>
    private void Dispatch(Action action)
    {
        if (Dispatcher is null)
        {
            action();
        }
        else
        {
            Dispatcher.Invoke(action);
        }
    }

    /// <summary>
    /// 一つ前のメッセージから続いたメッセージかどうかを判定する
    /// </summary>
    /// <param name="chatMessageType"></param>
    /// <param name="channel"></param>
    /// <param name="username"></param>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    private bool IsConsecutiveMessage(ChatMessageType chatMessageType, string channel, string username, DateTime timestamp)
    {
        if (Messages.Count == 0)
        {
            return false;
        }
        var last = Messages.Last();
        return
            last.MessageType == chatMessageType
            && last.Channel == channel
            && last.Username == username
            && (timestamp - last.Timestamp).TotalSeconds <= _MessageCooldown;
    }

    public void Clear()
    {
        Dispatch(() => Messages.Clear());
    }
}