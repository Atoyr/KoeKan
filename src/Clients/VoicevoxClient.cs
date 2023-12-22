using System.IO;
using System.Net.Http;
using System.Media;

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
        try
        {
            _version = await _httpClient.GetStringAsync(_url + _versionPath);
            if (OnReady is not null)
            {
                await OnReady.Invoke();
            }
        }
        catch
        {
            throw;
        }

        _heartbeat = new Heartbeat(new TimeSpan(0, 1, 0));
        _heartbeat.OnHeartbeat += OnHeartbeat;
    }


    public Task StopAsync()
    {
        _version = null;
        return Task.CompletedTask;
    }

    public async Task SpeakMessageAsync(string message)
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
        player.Play();
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