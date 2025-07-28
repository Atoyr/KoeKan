namespace Medoz.KoeKan.Speakers;

public interface ISpeaker
{
    string Name { get; }
    bool IsRunning { get; }
    event Func<Task>? OnReady;

    event Action? OnDisposing;

    Task RunAsync();
    Task StopAsync();
    Task SpeakMessageAsync(string message);
}