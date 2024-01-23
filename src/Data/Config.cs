using System.IO;
using System.Text.Json;

namespace Medoz.MessageTransporter.Data;

/// <summary>
/// </summary>
public class Config : ConfigBase
{
    public string Username { get; set; }

    public string? Icon { get; set; }

    public double Width { get; set; } = 400;

    public double Height { get; set; } = 800;

    public string ModKey { get; set; } = "CONTROL";
    public string Key { get; set; } = "ENTER";

    public DiscordConfig Discord { get; set; } = null!;

    public TwitchConfig Twitch { get; set; }

    public VoicevoxConfig Voicevox { get; set; }

    public IEnumerable<string> Applications { get; set; } = new List<string>();

    protected static string _filePath { get => Path.Combine(_folderPath, "config.json"); }

    public Config()
    { 
        Username = "";
        Discord = new(null, false, null);
        Twitch = new(Enumerable.Empty<string>(), false, null);
        Voicevox = new(1, null);
    }

    public void Save() => Save<Config>(_filePath);

    public static Config? Load()
    {
        if (File.Exists(_filePath))
        {
            string json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<Config>(json);
        }
        return null;
    }
}