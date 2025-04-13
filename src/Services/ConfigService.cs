using System.IO;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Unicode;

using Medoz.KoeKan.Data;
using System.Runtime.CompilerServices;

namespace Medoz.KoeKan.Services;

public class ConfigService : IConfigService
{
    private readonly string _configFileName = "config.json";
    private Config? _config = null;

    private readonly string _secretFileName = "secret";
    private Secret? _secret = null;

    public ConfigService() { }

    public ConfigService(Config config)
    {
        _config = config;
    }

    public Config GetConfig()
    {
        if (_config is null)
        {
            _config = LoadConfig(_configFileName);
            if (_config is null)
            {
                _config = new Config();
                Save();
            }
        }
        return _config;
    }

    public void Save()
    {
        SaveConfig(GetConfig(), _configFileName);
        SaveSecret(GetSecret(), _secretFileName);
    }

    protected string _folderPath
    {
        get => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ApplicationInfo.ApplicationName, "config");
    }

    private Config? LoadConfig(string fileName)
    {
        string filePath = Path.Combine(_folderPath, "secret.json");
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            var config = JsonSerializer.Deserialize<Config>(json);
            if (config is not null)
            {
                return config;
            }
        }
        return null;
    }

    public void SaveConfig()
    {
        if (_config is null)
        {
            _config = new Config();
        }
        SaveConfig(_config, _configFileName);
    }

    private void SaveConfig(Config source, string fileName)
    {
        Directory.CreateDirectory(_folderPath); // フォルダが存在しない場合は作成

        string json = JsonSerializer.Serialize(source);
        string filePath = Path.Combine(_folderPath, fileName);
        File.WriteAllText(filePath, json);
    }

    public Secret GetSecret()
    {
        string filePath = Path.Combine(_folderPath, "secret.json");
        return LoadSecret(filePath);
    }

    private Secret LoadSecret(string filePath)
    {
        if (_secret is not null)
        {
            return _secret;
        }

        if (File.Exists(filePath))
        {
            string credentialString = File.ReadAllText(filePath);
            string json = Credential.DecryptString(credentialString);
            var secret = JsonSerializer.Deserialize<Secret>(json);
            if (secret is not null)
            {
                _secret = secret;
                return _secret;
            }
        }
        _secret = new Secret();
        return _secret;
    }

    public void SaveSecret()
    {
        if (_secret is null)
        {
            _secret = new Secret();
        }
        SaveSecret(_secret, _secretFileName);
    }

    private void SaveSecret(Secret source, string fileName)
    {
        Directory.CreateDirectory(_folderPath); // フォルダが存在しない場合は作成

        JsonSerializerOptions jsonSerializerOption = new()
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
            WriteIndented = true,
            IncludeFields = true,
        };

        string json = JsonSerializer.Serialize(source, source.GetType(), jsonSerializerOption);
        string credentialString = Credential.EncryptString(json);
        string filePath = Path.Combine(_folderPath, fileName);
        File.WriteAllText(filePath, credentialString);
    }
}

