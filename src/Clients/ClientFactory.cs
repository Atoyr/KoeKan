using Discord;
using Discord.Audio;
using Discord.WebSocket;

namespace Medoz.KoeKan.Clients;

public class ClientFactory
{
    public T Create<T>(IClientOptions options) where T : IClient
    {
        return (T)Create(typeof(T), options);
    }

    public IClient Create(Type clientType, IClientOptions options)
    {
        if (!typeof(IClient).IsAssignableFrom(clientType))
        {
            throw new ArgumentException($"Type {clientType.FullName} does not implement IClient interface.", nameof(clientType));
        }

        var client = Activator.CreateInstance(clientType, options);

        if (client is null)
        {
            throw new InvalidOperationException($"Could not create instance of {clientType.FullName}.");
        }

        if (client is not IClient)
        {
            throw new InvalidCastException($"Created instance of {clientType.FullName} does not implement IClient interface.");
        }

        return (IClient)client;
    }
}
