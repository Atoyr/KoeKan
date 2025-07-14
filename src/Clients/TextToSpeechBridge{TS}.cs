using System.Reflection;

namespace Medoz.KoeKan.Clients;

public class TextToSpeechBridge<T, S> : ITextClient where T : ITextClient, new() where S : ISpeakerClient, new()
{
    private readonly T? _client;

    private readonly S ? _speaker;

    public bool IsRunning => _client!.IsRunning && _speaker!.IsRunning;

    public event Func<Task>? OnReady;

    public string Name => GetType().Name;

    public async Task RunAsync()
    {
        ValidateClient();
        ValidateSpeaker();
        List<Task> tasks = new();
        if (!_client!.IsRunning)
        {
            tasks.Add(_client!.RunAsync());
        }
        if (!_speaker!.IsRunning)
        {
            tasks.Add(_speaker!.RunAsync());
        }
        await Task.WhenAll(tasks);
    }

    public async Task StopAsync()
    {
        ValidateClient();
        await _client!.StopAsync();
    }

    private void ValidateClient()
    {
        if (_client is null)
        {
            throw new InvalidOperationException("Failed to create an instance of the client.");
        }
    }

    private void ValidateSpeaker()
    {
        if (_speaker is null)
        {
            throw new InvalidOperationException("Failed to create an instance of the speaker.");
        }
    }

    public TextToSpeechBridge(ITextClient client, IClientOptions speakerOptions)
    {
        if (typeof(T) != client.GetType())
        {
            throw new ArgumentException("The client type does not match the specified type.");
        }

        _client = (T)client;
        _speaker = CreateInstance<S>(speakerOptions);

        ValidateClient();

        _client!.OnReady += (async () => {
            if (OnReady is not null)
            {
                await OnReady.Invoke();
            }
        });
        _client!.OnReceiveMessage += async (message) =>
        {
            ValidateSpeaker();
            await _speaker!.SpeakMessageAsync(message.Content);
        };
    }

    public TextToSpeechBridge(IClientOptions textOptions, IClientOptions speakerOptions)
    {
        _client = CreateInstance<T>(textOptions);
        _speaker = CreateInstance<S>(speakerOptions);

        ValidateClient();

        _client!.OnReady += (async () => {
            if (OnReady is not null)
            {
                await OnReady.Invoke();
            }
        });
        _client!.OnReceiveMessage += async (message) =>
        {
            ValidateSpeaker();
            await _speaker!.SpeakMessageAsync(message.Content);
        };
    }

    /// <summary>
    /// IOptionsインターフェースを受け取るコンストラクタを探す
    /// </summary>
    private ConstructorInfo? FindOptionsConstructor(ConstructorInfo[] constructors)
    {
        foreach (var constructor in constructors)
        {
            var parameters = constructor.GetParameters();
            // IClientOptionsのみを受け取るコンストラクタがあるかチェック
            if (parameters.Length != 1)
            {
                continue;
            }

            var parameter = parameters[0];
            Type paramType = parameter.ParameterType;
            var interfacename = typeof(IClientOptions).Name;

            if (paramType.IsInterface && paramType.Name.StartsWith(interfacename))
            {
                return constructor;
            }

            // もしくはIOptionsを実装しているかチェック
            if ( paramType.GetInterfaces().Any(i => i.Name.StartsWith(interfacename)))
            {
                return constructor;
            }
        }
        return null;
    }

    private C? CreateInstance<C>(IClientOptions options)
    {
        var optionsConstructor = FindOptionsConstructor(typeof(C).GetConstructors());

        if (optionsConstructor is not null)
        {
            return (C?)Activator.CreateInstance(typeof(C), options);
        }
        else
        {
            return Activator.CreateInstance<C>();
        }
    }

    public event Func<ClientMessage, Task>? OnReceiveMessage
    {
        add
        {
            ValidateClient();
            _client!.OnReceiveMessage += value;
        }
        remove
        {
            ValidateClient();
            _client!.OnReceiveMessage -= value;
        }
    }


    public Task SendMessageAsync(ClientMessage message)
    {
        ValidateClient();
        return _client!.SendMessageAsync(message);
    }

    public void Dispose() => _client?.Dispose();
}