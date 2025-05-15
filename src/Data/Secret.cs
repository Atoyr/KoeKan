using System.IO;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Text.Json.Serialization;

namespace Medoz.KoeKan.Data;

/// <summary>
/// シークレット情報を保持するクラス
/// </summary>
public class Secret
{
    [JsonInclude]
    private readonly Dictionary<string, string> _secretDictionary = new();

    /// <summary>
    /// シークレットの値を取得します。
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public string GetValue(string key)
    {
        if (_secretDictionary.TryGetValue(key, out var value))
        {
            return value;
        }
        return string.Empty;
    }

    /// <summary>
    /// シークレットの値を設定します。
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void SetValue(string key, string value)
    {
        if (_secretDictionary.ContainsKey(key))
        {
            _secretDictionary[key] = value;
        }
        else
        {
            _secretDictionary.Add(key, value);
        }
    }
}