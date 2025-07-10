using System.CodeDom;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Medoz.KoeKan.Data;

/// <summary>
/// </summary>
[JsonConverter(typeof(DynamicConfigConverter))]
public class DynamicConfig
{
    private readonly Dictionary<string, object?> _configValues = new();

    public object? this[string key]
    {
        get => GetValue<object>(key);
        set => SetValue(key, value);
    }

    private void SetValue(string key, object? value)
    {
        if (_configValues.ContainsKey(key))
        {
            _configValues[key] = value;
        }
        else
        {
            _configValues.Add(key, value);
        }
    }

    // ジェネリックメソッドを使用して型安全に値を取得
    public T? GetValue<T>(string key)
    {
        if (_configValues.TryGetValue(key, out var value))
        {
            return ConvertValue<T>(value);
        }
        throw new KeyNotFoundException($"Configuration key '{key}' not found.");
    }

    // 取得を試行するメソッド (失敗しても例外を投げない)
    public bool TryGetValue<T>(string key, out T? value)
    {
        value = default;
        if (_configValues.TryGetValue(key, out var objValue))
        {
            try
            {
                value = ConvertValue<T>(objValue);
                return true;
            }
            catch
            {
                return false;
            }
        }
        return false;
    }

    // 値を適切な型に変換するヘルパーメソッド
    private T? ConvertValue<T>(object? value)
    {
        if (value is T typedValue)
        {
            return typedValue;
        }

        if (value is null)
        {
            return default;
        }

        // JsonSerializerを使用して型変換を試みる（複雑な型変換に対応）
        var json = JsonSerializer.Serialize(value);
        return JsonSerializer.Deserialize<T>(json) ?? default!;
    }
}

