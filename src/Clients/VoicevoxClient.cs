using System.IO;
using System.Net.Http;
using System.Media;
using System.Threading.Channels;

namespace Medoz.MessageTransporter.Clients;

public class VoicevoxClient: ISpeakerClient
{
    private string _url = "http://127.0.0.1:50021";

    private static string _versionPath = "/version";

    private static string _audioQuery = "/audio_query";

    private static string _synthesis = "/synthesis";

    private HttpClient _httpClient = new();

    private string? _version;

    private Heartbeat? _heartbeat;

    private uint _speakerId;

    private Channel<string>? _messageChannel;

    private CancellationTokenSource _messageCancelTokenSource = new();

    public VoicevoxClient(uint speakerId, string? url = null)
    {
        if (url is not null)
        {
            _url = url;
        }
        _speakerId = speakerId;
    }

    public void Dispose()
    {
        _heartbeat?.Dispose();
        _httpClient.Dispose();
    }

    public event Func<Task>? OnReady;

    public async Task RunAsync()
    {
        if (!string.IsNullOrEmpty(_version))
        {
            throw new Exception("VOICEVOX is running");
        }

        try
        {
            _version = await _httpClient.GetStringAsync(_url + _versionPath);
        }
        catch
        {
            throw;
        }

        _heartbeat = new Heartbeat(new TimeSpan(0, 1, 0));
        _heartbeat.OnHeartbeat += OnHeartbeat;
        _messageChannel = Channel.CreateUnbounded<string>();
        RunChannelReaderAsync();

        if (OnReady is not null)
        {
            await OnReady.Invoke();
        }
    }

    public Task StopAsync()
    {
        _version = null;
        if (_heartbeat is not null)
        {
            _heartbeat.OnHeartbeat -= OnHeartbeat;
            _heartbeat = null;
        }
        if (_messageChannel is not null)
        {
            _messageCancelTokenSource.Cancel();
            _messageChannel = null;
        }
        return Task.CompletedTask;
    }

    public async Task SpeakMessageAsync(string message)
    {
        if (_messageChannel is null) throw new NullReferenceException("Channel is null");
        await _messageChannel.Writer.WriteAsync(message);
    }

    public void SetSpeakerId(uint speakerId)
    {
        // TODO speakerIdのチェック
        _speakerId = speakerId;
    }

    private async Task RunChannelReaderAsync()
    {
        if (_messageChannel is null) throw new NullReferenceException("Channel is null");

        // 外部から変数を変更するとおかしな挙動になるかも？ロックしていないので、要注意
        await foreach(var message in _messageChannel.Reader.ReadAllAsync().WithCancellation(_messageCancelTokenSource.Token))
        {
            string encodedMessage = System.Net.WebUtility.UrlEncode(message);
            var audioQueryResponse = await _httpClient.PostAsync($"{_url}{_audioQuery}?text={encodedMessage}&speaker={_speakerId}", null);
            if (audioQueryResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"audio_query response is {audioQueryResponse.StatusCode}");
            }

            var voiceResponse = await _httpClient.PostAsync($"{_url}{_synthesis}?speaker={_speakerId}", audioQueryResponse.Content);
            var content = await voiceResponse.Content.ReadAsByteArrayAsync();

            using var stream = new MemoryStream(content);
            using var player = new SoundPlayer(stream);
            player.PlaySync();
        }
    }

    private async void OnHeartbeat(object? _)
    {
        try
        {
            using HttpClient clinet = new();
            _version = await clinet.GetStringAsync(_url + _versionPath);
        }
        catch
        {
            _version = null;
        }
    }
}