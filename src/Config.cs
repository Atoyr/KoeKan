using System.IO;
using System.Text.Json;

namespace Medoz.TextTransporter;

/// <summary>
/// </summary>
public class Config
{
    public const string APP_NAME = "TextTransporter";

    public string Token { get; set; } = null!;

    public IEnumerable<string> Applications { get; set; }

    private string _folderPath { get => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", APP_NAME); }

    public Config? Load()
    {

    }
    
    public void Save()
    {
        Directory.CreateDirectory(_folderPath); // フォルダが存在しない場合は作成

        string filePath = Path.Combine(folderPath, fileName);
        string json = JsonSerializer.Serialize(this);
        File.WriteAllText(filePath, json);
    }
}