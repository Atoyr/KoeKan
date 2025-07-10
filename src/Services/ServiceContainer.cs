using Medoz.KoeKan.Clients;
using Medoz.KoeKan.Data;

namespace Medoz.KoeKan.Services;

internal class ServiceContainer
{

#region Region: Singleton Implementation
    private static ServiceContainer? _instance;

    public static ServiceContainer Instance => _instance ??= new ServiceContainer();
#endregion

#region ClientService
    private Type _clientType = typeof(ClientService);
    public Type ClientType
    {
        get => _clientType;
        set
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value), "ClientType cannot be null.");
            }
            if (!typeof(IClientService).IsAssignableFrom(value))
            {
                throw new ArgumentException("ClientType must inherit from IClientService.", nameof(value));
            }
            _clientType = value;
        }
    }
    private IClientService? _clientService;
    public IClientService ClientService
    {
        get
        {
            if (_clientService is null)
            {
                _clientService = (IClientService)(Activator.CreateInstance(ClientType) ??
                    throw new InvalidOperationException($"Could not create an instance of {ClientType.FullName}. Ensure it has a parameterless constructor and implements IClientService."));
            }
            return _clientService;
        }
    }
#endregion

#region ListenerService
    private Type _listenerType = typeof(ListenerService);
    public Type ListenerType
    {
        get => _listenerType;
        set
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value), "ListenerType cannot be null.");
            }
            if (!typeof(IListenerService).IsAssignableFrom(value))
            {
                throw new ArgumentException("ListenerType must inherit from IListenerService.", nameof(value));
            }
            _listenerType = value;
        }
    }
    private IListenerService? _listenerService;
    public IListenerService ListenerService
    {
        get
        {
            if (_listenerService is null)
            {
                _listenerService = (IListenerService)(Activator.CreateInstance(ListenerType) ??
                    throw new InvalidOperationException($"Could not create an instance of {ListenerType.FullName}. Ensure it has a parameterless constructor and implements IListenerService."));
            }
            return _listenerService;
        }
    }
#endregion

#region ConfigService
    private Type _configType = typeof(ConfigService);
    public Type ConfigType
    {
        get => _configType;
        set
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value), "ConfigType cannot be null.");
            }
            if (!typeof(IConfigService).IsAssignableFrom(value))
            {
                throw new ArgumentException("ConfigType must inherit from IConfigService.", nameof(value));
            }
            _configType = value;
        }
    }
    private IConfigService? _configService;
    public IConfigService ConfigService
    {
        get
        {
            if (_configService is null)
            {
                _configService = (IConfigService)(Activator.CreateInstance(ConfigType) ??
                    throw new InvalidOperationException($"Could not create an instance of {ConfigType.FullName}. Ensure it has a parameterless constructor and implements IConfigService."));
            }
            return _configService;
        }
    }
#endregion

#region ServerService
    private Type _serverType = typeof(ServerService);
    public Type ServerType
    {
        get => _serverType;
        set
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value), "ServerType cannot be null.");
            }
            if (!typeof(IServerService).IsAssignableFrom(value))
            {
                throw new ArgumentException("ServerType must inherit from IServerService.", nameof(value));
            }
            _serverType = value;
        }
    }
    private IServerService? _serverService;
    public IServerService ServerService
    {
        get
        {
            if (_serverService is null)
            {
                _serverService = (IServerService)(Activator.CreateInstance(ServerType) ??
                    throw new InvalidOperationException($"Could not create an instance of {ServerType.FullName}. Ensure it has a parameterless constructor and implements IServerService."));
            }
            return _serverService;
        }
    }
#endregion

#region WindowService
    private Type _windowType = typeof(WindowService);
    public Type WindowType
    {
        get => _windowType;
        set
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value), "WindowType cannot be null.");
            }
            if (!typeof(IWindowService).IsAssignableFrom(value))
            {
                throw new ArgumentException("WindowType must inherit from IWindowService.", nameof(value));
            }
            _windowType = value;
        }
    }
    private IWindowService? _windowService;
    public IWindowService WindowService
    {
        get
        {
            if (_windowService is null)
            {
                _windowService = (IWindowService)(Activator.CreateInstance(WindowType) ??
                    throw new InvalidOperationException($"Could not create an instance of {WindowType.FullName}. Ensure it has a parameterless constructor and implements IWindowService."));
            }
            return _windowService;
        }
    }
#endregion

}