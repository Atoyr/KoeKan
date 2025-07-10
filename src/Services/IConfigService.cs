
using Medoz.KoeKan.Clients;
using Medoz.KoeKan.Data;

namespace Medoz.KoeKan.Services;

public interface IConfigService
{
    void Save();
    void Reload();
    Config GetConfig();
    void SaveConfig();
    event EventHandler? ConfigChanged;

    Secret GetSecret();
    void SaveSecret();

    event EventHandler? SecretChanged;
}
