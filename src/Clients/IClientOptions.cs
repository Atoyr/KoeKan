using System.CodeDom;

namespace Medoz.KoeKan.Clients;

public interface IClientOptions
{
    string this[string key] { get; set; }
}
