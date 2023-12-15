namespace Medoz.MessageTransporter.Clients;

public interface ITextClient: IClient
{
    event Func<Message, Task>? OnReceiveMessage;
    Task SendMessageAsync(string message);
}
