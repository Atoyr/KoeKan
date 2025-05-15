using System.IO;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Unicode;

using Medoz.KoeKan.Data;

namespace Medoz.KoeKan.Services;

/// <summary>
/// 設定情報を管理するクラス
/// </summary>
public class ConfigService : IConfigService
{
    /// <summary>
    /// 設定ファイルの名前
    /// </summary>
    private readonly string _configFileName = "config.json";
    private Config? _config = null;

    /// <summary>
    /// シークレットファイルの名前
    /// </summary>
    private readonly string _secretFileName = "secret";
    private Secret? _secret = null;

    public ConfigService() { }

    public ConfigService(Config config)
    {
        _config = config;
    }

    /// <summary>
    /// 設定情報を取得します。
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// 設定とシークレット情報を保存します。
    /// </summary>
    public void Save()
    {
        SaveConfig(GetConfig(), _configFileName);
        SaveSecret(GetSecret(), _secretFileName);
    }

    /// <summary>
    /// 設定ファイルのパスを取得します。
    /// </summary>
    protected string _folderPath
    {
        get => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ApplicationInfo.ApplicationName, "config");
    }

    /// <summary>
    /// 設定ファイルを読み込みます。
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    private Config? LoadConfig(string fileName)
    {
        string filePath = Path.Combine(_folderPath, fileName);
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

    /// <summary>
    /// 設定ファイルを保存します。
    /// </summary>
    public void SaveConfig()
    {
        if (_config is null)
        {
            _config = new Config();
        }
        SaveConfig(_config, _configFileName);
    }

    /// <summary>
    /// 設定ファイルを保存します。
    /// </summary>
    /// <param name="source"></param>
    /// <param name="fileName"></param>
    private void SaveConfig(Config source, string fileName)
    {
        Directory.CreateDirectory(_folderPath); // フォルダが存在しない場合は作成

        string json = JsonSerializer.Serialize(source);
        string filePath = Path.Combine(_folderPath, fileName);
        File.WriteAllText(filePath, json);
    }

    /// <summary>
    /// シークレット情報を取得します。
    /// </summary>
    /// <returns></returns>
    public Secret GetSecret()
    {
        string filePath = Path.Combine(_folderPath, "secret.json");
        return LoadSecret(filePath);
    }

    /// <summary>
    /// シークレット情報をファイルから読込みます。
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
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

    /// <summary>
    /// シークレット情報をファイルに保存します。
    /// </summary>
    public void SaveSecret()
    {
        if (_secret is null)
        {
            _secret = new Secret();
        }
        SaveSecret(_secret, _secretFileName);
    }

    /// <summary>
    /// シークレット情報をファイルに保存します。
    /// </summary>
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
        string credentialString = Credential.EncryptString(json); // 暗号化
        string filePath = Path.Combine(_folderPath, fileName);
        File.WriteAllText(filePath, credentialString);
    }
}

