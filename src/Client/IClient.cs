namespace Medoz.TextTransporter.Client;

public interface IClient: IDisposable
{
    event Func<Message, Task>? OnReceiveMessage;

    Task RunAsync();
    Task StopAsync();
}