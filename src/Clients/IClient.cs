namespace Medoz.KoeKan.Clients;

public interface IClient: IDisposable
{
    string Name { get; }
    bool IsRunning { get; }
    event Func<Task>? OnReady;

    event Action? OnDisposing;

    Task RunAsync();
    Task StopAsync();
}