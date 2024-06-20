using System.IO;
using System.Text.Json;

namespace Medoz.KoeKan.Data;

/// <summary>
/// </summary>
public class Config : ConfigBase
{
    private object _lock = new();

    private string _username = "";
    public string Username
    {
        get
        {
            lock(_lock)
            {
                return _username;
            }
        }
        set
        {
            lock(_lock)
            {
                _username = value;
            }
        }
    }

    private string? _icon;
    public string? Icon 
    {
        get
        {
            lock(_lock)
            {
                return _icon;
            }
        }
        set
        {
            lock(_lock)
            {
                _icon = value;
            }
        }
    }

    private double _width = 400;
    public double Width 
    {
        get
        {
            lock(_lock)
            {
                return _width;
            }
        }
        set
        {
            lock(_lock)
            {
                _width = value;
            }
        }
    }

    private double _height = 800;
    public double Height 
    {
        get
        {
            lock(_lock)
            {
                return _height;
            }
        }
        set
        {
            lock(_lock)
            {
                _height = value;
            }
        }
    }

    private string _modKey = "CONTROL";
    private string _key = "ENTER";
    public string ModKey 
    {
        get
        {
            lock(_lock)
            {
                return _modKey;
            }
        }
        set
        {
            lock(_lock)
            {
                _modKey = value;
            }
        }
    }
    public string Key
    {
        get
        {
            lock(_lock)
            {
                return _key;
            }
        }
        set
        {
            lock(_lock)
            {
                _key = value;
            }
        }
    }

    private DiscordConfig _discord = new(null, false, null);
    public DiscordConfig Discord 
    {
        get
        {
            lock(_lock)
            {
                return _discord;
            }
        }
        set
        {
            lock(_lock)
            {
                _discord = value;
            }
        }
    }

    private TwitchConfig _twitch = new(Enumerable.Empty<string>(), false, null);
    public TwitchConfig Twitch 
    {
        get
        {
            lock(_lock)
            {
                return _twitch;
            }
        }
        set
        {
            lock(_lock)
            {
                _twitch = value;
            }
        }
    }

    private VoicevoxConfig _voicevox = new(1, null);
    public VoicevoxConfig Voicevox 
    {
        get
        {
            lock(_lock)
            {
                return _voicevox;
            }
        }
        set
        {
            lock(_lock)
            {
                _voicevox = value;
            }
        }
    }

    private IEnumerable<string> _applications = new List<string>();
    public IEnumerable<string> Applications 
    {
        get
        {
            lock(_lock)
            {
                return _applications;
            }
        }
        set
        {
            lock(_lock)
            {
                _applications = value;
            }
        }
    }

    public Config()
    { 
    }

    public void Save()
    {
        lock(_lock)
        {
            Save<Config>(_filePath);
        }
    }

    protected static string _filePath { get => Path.Combine(_folderPath, "config.json"); }

    private static Config? _instance;
    private static object _loadLock = new();

    public static Config Load()
    {
        lock(_loadLock)
        {
            if (_instance is null)
            {
                if (File.Exists(_filePath))
                {
                    string json = File.ReadAllText(_filePath);
                    _instance = JsonSerializer.Deserialize<Config>(json);
                }
                else 
                {
                    _instance = new();
                }
            }
            return _instance;
        }
    }
}