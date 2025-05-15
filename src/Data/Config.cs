using System.IO;
using System.Text.Json;

namespace Medoz.KoeKan.Data;

/// <summary>
/// </summary>
public class Config
{
    public Config()
    {
    }

    private readonly object _lock = new();

    public IDictionary<string, DynamicConfig> Clients
    {
        get;
        // FIXME: Json形式で保存するために、setをpublicにしているが、
        // できればprivateにしたい。

        set;
    } = new Dictionary<string, DynamicConfig>();

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
}