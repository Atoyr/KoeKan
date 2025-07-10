using Medoz.KoeKan.Clients;
using Medoz.KoeKan.Data;

namespace Medoz.KoeKan.Services;

public interface IListenerService
{
    Listener GetListener();
    void AddLogMessage(string message, string? logLevel = null);
    void AddCommandMessage(string message);
    void AddMessage(ClientMessage message);
    void Clear();
}
