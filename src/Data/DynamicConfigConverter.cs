using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Medoz.KoeKan.Data;

// インデクサーを持つクラス用のカスタムJsonConverter
public class DynamicConfigConverter : JsonConverter<DynamicConfig>
{
    // シリアライズメソッド
    public override void Write(Utf8JsonWriter writer, DynamicConfig value, JsonSerializerOptions options)
    {

        // リフレクションを使ってプライベートフィールドにアクセス
        var field = typeof(Config).GetField("_configValues",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var configValues = field!.GetValue(value) as Dictionary<string, object>;

        // オブジェクト開始
        writer.WriteStartObject();

        // 辞書の各項目を個別のプロパティとして書き込む
        foreach (var kvp in configValues!)
        {
            // キーをプロパティ名として書き込む
            writer.WritePropertyName(kvp.Key);

            // 値の型に応じて適切に書き込む
            WriteValue(writer, kvp.Value, options);
        }

        // オブジェクト終了
        writer.WriteEndObject();
    }

    // 値を適切な型でシリアライズするヘルパーメソッド
    private void WriteValue(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        // nullの場合
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        // 値の型に応じて適切なメソッドを呼び出す
        switch (value)
        {
            case string s:
                writer.WriteStringValue(s);
                break;
            case int i:
                writer.WriteNumberValue(i);
                break;
            case long l:
                writer.WriteNumberValue(l);
                break;
            case double d:
                writer.WriteNumberValue(d);
                break;
            case float f:
                writer.WriteNumberValue(f);
                break;
            case bool b:
                writer.WriteBooleanValue(b);
                break;
            case DateTime dt:
                writer.WriteStringValue(dt);
                break;
            default:
                // 他の複雑な型はJsonSerializerを使用して処理
                var json = JsonSerializer.Serialize(value, options);
                using (var doc = JsonDocument.Parse(json))
                {
                    doc.RootElement.WriteTo(writer);
                }
                break;
        }
    }

    // デシリアライズメソッド
    public override DynamicConfig Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("JSON object expected");
        }

        var config = new DynamicConfig();

        // JSONオブジェクトのすべてのプロパティを読み込む
        while (reader.Read())
        {
            // オブジェクトの終わりに達したら終了
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return config;
            }

            // プロパティ名を取得
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("Property name expected");
            }

            string propertyName = reader.GetString()!;

            // 次のトークンに進む
            reader.Read();

            // 値を型に応じて読み込む
            object value = ReadValue(ref reader, options);

            // インデクサーを使用して値を設定
            config[propertyName] = value;
        }

        throw new JsonException("Unexpected end of JSON");
    }

    // JSONから値を読み込むヘルパーメソッド
    private object ReadValue(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                return new();
            case JsonTokenType.True:
                return true;
            case JsonTokenType.False:
                return false;
            case JsonTokenType.Number:
                if (reader.TryGetInt32(out int intValue))
                {
                    return intValue;
                }
                if (reader.TryGetInt64(out long longValue))
                {
                    return longValue;
                }
                if (reader.TryGetDouble(out double doubleValue))
                {
                    return doubleValue;
                }
                throw new JsonException("Unsupported number format");
            case JsonTokenType.String:
                return reader.GetString() ?? "";
            case JsonTokenType.StartArray:
                return ReadArray(ref reader, options);
            case JsonTokenType.StartObject:
                return ReadObject(ref reader, options);
            default:
                throw new JsonException($"Unsupported token type: {reader.TokenType}");
        }
    }

    // 配列を読み込むヘルパーメソッド
    private List<object> ReadArray(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        var list = new List<object>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                return list;
            }

            var value = ReadValue(ref reader, options);
            list.Add(value);
        }

        throw new JsonException("Unexpected end of JSON array");
    }

    // オブジェクトを読み込むヘルパーメソッド
    private Dictionary<string, object> ReadObject(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        var dict = new Dictionary<string, object>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return dict;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("Property name expected");
            }

            string propertyName = reader.GetString()!;
            reader.Read();

            var value = ReadValue(ref reader, options);
            dict[propertyName] = value;
        }

        throw new JsonException("Unexpected end of JSON object");
    }
}