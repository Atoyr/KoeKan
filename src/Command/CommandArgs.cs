using Medoz.KoeKan.Clients;
using Medoz.KoeKan.Data;

namespace Medoz.KoeKan.Command;

public record CommandArgs
{
    private readonly string[] _args;
    private uint _index = 0;

    public string[] Args
    {
        get
        {
            if (_index  < _args.Length )
            {
                return _args[(int)_index..];
            }
            return Array.Empty<string>();
        }
    }

    public Listener Listener { get; init; }
    public IDictionary<string, ITextClient> Clients { get; init; }
    public Config Config { get; init; }

    public Dictionary<string, string> Options { get; init; } = new();

    public CommandArgs(string[] args, Config config, Listener listener, IDictionary<string, ITextClient> clients)
    {
        _args = args;
        Config = config;
        Listener = listener;
        Clients = clients;
    }

    public void Next()
    {
        _index++;
    }

    public void ResetIndex()
    {
        _index = 0;
    }
}
