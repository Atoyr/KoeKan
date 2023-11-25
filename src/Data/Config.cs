using System.IO;
using System.Text.Json;

using Medoz.MessageTransporter.Clients;


namespace Medoz.MessageTransporter.Data;

/// <summary>
/// </summary>
public class Config
{
    public const string APP_NAME = "MessageTransporter";

    public DiscordOptions Discord { get; set; } = null!;

    public IEnumerable<string> Applications { get; set; } = new List<string>();

    private static string _folderPath { get => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", APP_NAME); }
    private static string _filePath { get => Path.Combine(_folderPath, "config.json"); }

    public IEnumerable<string> Processes { get; init; } = new List<string>();

    public Config()
    { 
        Discord = new("", "", "");
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