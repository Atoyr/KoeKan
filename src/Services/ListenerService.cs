using Medoz.KoeKan.Clients;
using Medoz.KoeKan.Data;

namespace Medoz.KoeKan.Services;

public class ListenerService : IListenerService
{
    private readonly Listener _listener;
    public ListenerService(): this(new Listener())
    {
    }

    public ListenerService(Listener listener)
    {
        _listener = listener;
    }

    public Listener GetListener()
    {
        return _listener;
    }

    public void AddLogMessage(string message, string? logLevel = "Info")
    {
        // FIXME: LogLevel is not used in the original code, but it is used in the interface.
        _listener.AddLogMessage(ChatMessageType.LogInfo, message);
    }

    public void AddCommandMessage(string message)
    {
        _listener.AddCommandMessage(message);
    }

    public void AddMessage(ClientMessage message)
    {
        _listener.AddMessage(message);
    }

    public void Clear()
    {
        _listener.Messages.Clear();
    }
}

