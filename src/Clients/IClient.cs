namespace Medoz.KoeKan.Clients;

public interface IClient: IDisposable
{
    event Func<Task>? OnReady;

    Task RunAsync();
    Task StopAsync();
}