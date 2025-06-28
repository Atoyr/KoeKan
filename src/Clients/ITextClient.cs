namespace Medoz.KoeKan.Clients;

public interface ITextClient: IClient
{
    event Func<ClientMessage, Task>? OnReceiveMessage;
    Task SendMessageAsync(ClientMessage message);
}