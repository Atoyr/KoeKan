using System.IO;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace Medoz.MessageTransporter.Data;

/// <summary>
/// </summary>
public class Secret : ConfigBase
{
    private static object _lock = new();
    private static JsonSerializerOptions _jsonSerializerOption = new JsonSerializerOptions
    {
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
        WriteIndented = true
    };

    public string? Twitch { get; set; }

    public string? DectyptTwitch()
    {
        if(Twitch is null)
        {
            return Twitch;
        }
        return Credential.DecryptString(Twitch);
    }

    public void EncryptTwitch(string value)
    {
        if (value is null)
        {
            Twitch = null;
            return;
        }
        Twitch = Credential.EncryptString(value);
    }

    public string? Discord { get; set; }

    public string? DectyptDiscord()
    {
        if(Discord is null)
        {
            return Discord;
        }
        return Credential.DecryptString(Discord);
    }

    public void EncryptDiscord(string value)
    {
        if (value is null)
        {
            Discord = null;
            return;
        }
        Discord = Credential.EncryptString(value);
    }

    private static string _filePath { get => Path.Combine(_folderPath, "secret.json"); }

    private static Secret? _secret;

    public Secret()
    { 
    }

    public void Save() 
    {
        lock(_lock)
        {
            Directory.CreateDirectory(_folderPath); // フォルダが存在しない場合は作成

            string json = JsonSerializer.Serialize(this, _jsonSerializerOption);
            File.WriteAllText(_filePath, json);
        }
    }

    public static Secret Load()
    {
        lock(_lock)
        {
            if (_secret is null)
            {
                if (File.Exists(_filePath))
                {
                    string json = File.ReadAllText(_filePath);
                    var secret = JsonSerializer.Deserialize<Secret>(json, _jsonSerializerOption);
                    if (secret is not null)
                    {
                        _secret = secret;
                        return _secret;
                    }
                }
                return new();
            }
            else
            {
                return _secret;
            }
        }
    }
}