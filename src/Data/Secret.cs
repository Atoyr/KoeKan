using System.IO;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Text.Json.Serialization;

namespace Medoz.KoeKan.Data;

/// <summary>
/// </summary>
public class Secret
{
    [JsonInclude]
    private readonly Dictionary<string, string> _secretDictionary = new();

    public string GetValue(string key)
    {
        if (_secretDictionary.TryGetValue(key, out var value))
        {
            return value;
        }
        return string.Empty;
    }
}