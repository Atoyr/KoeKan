namespace Medoz.MessageTransporter.Clients;

public interface IClient: IDisposable
{
    event Func<Message, Task>? OnReceiveMessage;
    event Func<Task>? OnReady;

    Task RunAsync();
    Task StopAsync();
    Task SendMessageAsync(string message);
}