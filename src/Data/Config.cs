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

    /// <summary>
    /// クライアントの設定
    /// </summary>
    public IDictionary<string, DynamicConfig> Clients
    {
        get;

        // FIXME: Json形式で保存するために、setをpublicにしているが、
        // できればprivateにしたい。
        set;
    } = new Dictionary<string, DynamicConfig>();

    /// <summary>
    /// クライアントの設定
    /// </summary>
    public IDictionary<string, DynamicConfig> Speakers
    {
        get;

        // FIXME: Json形式で保存するために、setをpublicにしているが、
        // できればprivateにしたい。
        set;
    } = new Dictionary<string, DynamicConfig>();

    private string _username = "";
    /// <summary>
    /// 自身の名前
    /// </summary>
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
    /// <summary>
    /// 自身のアイコンファイルのパス
    /// </summary>
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
    /// <summary>
    /// ウィンドウの幅
    /// </summary>
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
    /// <summary>
    /// ウィンドウの高さ
    /// </summary>
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

    public double _x = 100;
    /// <summary>
    /// ウィンドウの左位置
    /// </summary>
    public double X
    {
        get
        {
            lock(_lock)
            {
                return _x;
            }
        }
        set
        {
            lock(_lock)
            {
                _x = value;
            }
        }
    }

    public double _y = 100;
    /// <summary>
    /// ウィンドウの上位置
    /// </summary>
    public double Y
    {
        get
        {
            lock(_lock)
            {
                return _y;
            }
        }
        set
        {
            lock(_lock)
            {
                _y = value;
            }
        }
    }



    private string _modKey = "CONTROL";
    private string _key = "ENTER";

    /// <summary>
    /// ホットキーの修飾キー
    /// </summary>
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

    /// <summary>
    /// ホットキーのキー
    /// </summary>
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
    /// <summary>
    /// アクティブに変更できるアプリケーション一覧
    /// </summary>
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