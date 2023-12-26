using System.IO;
using System.Text.Json;

namespace Medoz.MessageTransporter.Data;

/// <summary>
/// </summary>
public abstract class ConfigBase
{
    public const string APP_NAME = "MessageTransporter";

    protected static string _folderPath { get => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), APP_NAME, "config"); }

    public virtual void Save<T> (string filePath) where T: ConfigBase
    {
        Directory.CreateDirectory(_folderPath); // フォルダが存在しない場合は作成

        string json = JsonSerializer.Serialize((T)this);
        File.WriteAllText(filePath, json);
    }
}