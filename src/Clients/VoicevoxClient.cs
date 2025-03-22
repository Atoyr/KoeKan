using System.IO;
using System.Net.Http;
using System.Media;
using System.Threading.Channels;

namespace Medoz.KoeKan.Clients;

public class VoicevoxClient: ISpeakerClient
{
    private readonly string _url = "http://127.0.0.1:50021";

    private static readonly string _versionPath = "/version";

    private static readonly string _audioQuery = "/audio_query";

    private static readonly string _synthesis = "/synthesis";

    private readonly HttpClient _httpClient = new();

    private string? _version;

    private Heartbeat? _heartbeat;

    private uint _speakerId;

    private Channel<string>? _messageChannel;

    private readonly CancellationTokenSource _messageCancelTokenSource = new();

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

    public async Task<string> AuthAsync()
    {
        await _httpClient.GetStringAsync(_url + _versionPath);
        return "VOICEVOX";
    }

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

        // チャネル待ち受けなので、非同期で実行
        _ = RunChannelReaderAsync();

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
            // 音声変換用のデータを取得
            string encodedMessage = System.Net.WebUtility.UrlEncode(message);
            var audioQueryResponse = await _httpClient.PostAsync($"{_url}{_audioQuery}?text={encodedMessage}&speaker={_speakerId}", null);
            if (audioQueryResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"audio_query response is {audioQueryResponse.StatusCode}");
            }

            // オーディオデータを取得
            var voiceResponse = await _httpClient.PostAsync($"{_url}{_synthesis}?speaker={_speakerId}", audioQueryResponse.Content);
            var content = await voiceResponse.Content.ReadAsByteArrayAsync();

            // オーディオデータを再生
            playVoice(content);
        }
    }

    // 音声再生
    private void playVoice(byte[] content)
    {
        using var stream = new MemoryStream(content);
        using var player = new SoundPlayer(stream);
        player.PlaySync();
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