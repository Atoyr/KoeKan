namespace Medoz.KoeKan.Clients;

public interface IClient: IDisposable
{
    event Func<Task>? OnReady;

    Task<string> AuthAsync();

    Task RunAsync();
    Task StopAsync();
}