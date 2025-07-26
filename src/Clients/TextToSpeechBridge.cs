using System.Reflection;

namespace Medoz.KoeKan.Clients;

public class TextToSpeechBridge : ITextClient
{
    private readonly ITextClient _client;

    public ITextClient GetTextClient() => _client;

    private readonly ISpeakerClient _speaker;
    public bool IsRunning => _client!.IsRunning && _speaker!.IsRunning;

    public event Func<Task>? OnReady;
    public event Action? OnDisposing;

    public string Name => GetType().Name;

    public TextToSpeechBridge(ITextClient textClient, ISpeakerClient speakerClient)
    {
        _client = textClient;
        _speaker = speakerClient;

        _client!.OnReady += async () => {
            if (OnReady is not null)
            {
                await OnReady.Invoke();
            }
        };
        _client!.OnReceiveMessage += async (message) =>
        {
            await _speaker!.SpeakMessageAsync(message.Content);
        };
    }


    public async Task RunAsync()
    {
        List<Task> tasks = new();
        if (!_client.IsRunning)
        {
            tasks.Add(_client!.RunAsync());
        }
        if (!_speaker.IsRunning)
        {
            tasks.Add(_speaker!.RunAsync());
        }
        await Task.WhenAll(tasks);
    }

    public async Task StopAsync()
    {
        if (_speaker.IsRunning)
        {
            await _speaker!.StopAsync();
        }
        await _client!.StopAsync();
    }

    public async Task StopSeakerAsync()
    {
        await _speaker!.StopAsync();
    }

    public event Func<ClientMessage, Task>? OnReceiveMessage
    {
        add
        {
            _client!.OnReceiveMessage += value;
        }
        remove
        {
            _client!.OnReceiveMessage -= value;
        }
    }

    public Task SendMessageAsync(ClientMessage message)
    {
        return _client!.SendMessageAsync(message);
    }

    public void Dispose()
    {
        // クライアントは再利用する可能性があるのでDisposeしない
        _speaker?.Dispose();
    }
}
