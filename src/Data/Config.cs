using System.IO;
using System.Text.Json;

using Medoz.MessageTransporter.Clients;

namespace Medoz.MessageTransporter.Data;

/// <summary>
/// </summary>
public class Config
{
    public const string APP_NAME = "MessageTransporter";

    public DiscordConfig Discord { get; set; } = null!;

    public IEnumerable<TwitchConfig> Twitch { get; set; }

    public IEnumerable<string> Applications { get; set; } = new List<string>();

    private static string _folderPath { get => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", APP_NAME); }
    private static string _filePath { get => Path.Combine(_folderPath, "config.json"); }

    public Config()
    { 
        Discord = new("", null);
        Twitch = new List<TwitchConfig>();
    }

    public void Save()
    {
        Directory.CreateDirectory(_folderPath); // フォルダが存在しない場合は作成

        string json = JsonSerializer.Serialize(this);
        File.WriteAllText(_filePath, json);
    }

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