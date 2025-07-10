namespace Medoz.KoeKan.Clients;

public interface IClient: IDisposable
{
    string Name { get; }
    bool IsRunning { get; }
    event Func<Task>? OnReady;

    Task<string> AuthAsync();

    Task RunAsync();
    Task StopAsync();
}