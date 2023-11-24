using System.IO;
using System.Text.Json;

namespace Medoz.MessageTransporter.Data;

/// <summary>
/// </summary>
public class Config
{
    public const string APP_NAME = "MessageTransporter";

    public Discord Discord { get; set; }

    public IEnumerable<string> Applications { get; set; }

    private string _folderPath { get => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", APP_NAME); }

    public IEnumerable<string> Processes { get; init; } = new List<string>();

    public Config() { }

    public Config? Load()
    {
        return new Config();
    }
    
    public void Save()
    {
        Directory.CreateDirectory(_folderPath); // フォルダが存在しない場合は作成

        string filePath = Path.Combine(_folderPath, "config.json");
        string json = JsonSerializer.Serialize(this);
        File.WriteAllText(filePath, json);
    }
}