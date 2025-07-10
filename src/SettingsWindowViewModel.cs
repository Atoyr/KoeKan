using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Data;
using System.Windows.Threading;
using System.IO;
using System.ComponentModel;
using System.Windows.Input;

using Microsoft.Extensions.Logging;

using Medoz.KoeKan.Clients;
using Medoz.KoeKan.Data;
using Medoz.KoeKan.Services;
using Medoz.Logging;

namespace Medoz.KoeKan;

/// <summary>
/// Interaction logic for SettingsWindow.xaml
/// </summary>
public partial class SettingsWindowViewModel : INotifyPropertyChanged
{
    private readonly Config _config = ServiceContainer.Instance.ConfigService.GetConfig();
    public string MyName
    {
        get => _config.Username;
        set
        {
            if (_config.Username != value)
            {
                _config.Username = value;
                OnPropertyChanged(nameof(MyName));
            }
            SetPreviewMyName(value);
        }
    }

    public string IconPath
    {
        get => _config.Icon ?? string.Empty;
        set
        {
            if (_config.Icon != value)
            {
                _config.Icon = value;
                OnPropertyChanged(nameof(IconPath));
            }
            SetPreviewIconPath(value);
        }
    }

    private ChatMessage? _previewMessage = new ChatMessage(
                    ChatMessageType.Echo,
                    "",
                    null,
                    "",
                    "プレビュー",
                    new DateTime(2025, 7, 7, 7, 7, 7),
                    false);
    public ChatMessage? PreviewMessage
    {
        get
        {
            if (_previewMessage == null)
            {
                _previewMessage = new ChatMessage(
                    ChatMessageType.Echo,
                    "",
                    null,
                    MyName,
                    "プレビュー",
                    new DateTime(2025, 7, 7, 7, 7, 7),
                    false);
            }
            return _previewMessage;
        }
        set
        {
            if (_previewMessage != value)
            {
                _previewMessage = value;
                OnPropertyChanged(nameof(PreviewMessage));
            }
        }
    }

    public double Width
    {
        get => _config.Width;
        set
        {
            if (_config.Width != value)
            {
                _config.Width = value;
                OnPropertyChanged(nameof(Width));
            }
        }
    }

    public double Height
    {
        get => _config.Height;
        set
        {
            if (_config.Height != value)
            {
                _config.Height = value;
                OnPropertyChanged(nameof(Height));
            }
        }
    }

    public double X
    {
        get => _config.X;
        set
        {
            if (_config.X != value)
            {
                _config.X = value;
                OnPropertyChanged(nameof(X));
            }
        }
    }

    public double Y
    {
        get => _config.Y;
        set
        {
            if (_config.Y != value)
            {
                _config.Y = value;
                OnPropertyChanged(nameof(Y));
            }
        }
    }

    public MOD_KEY ModKey
    {
        get => ModKeyExtension.GetModKey(_config.ModKey);
        set
        {
            if (_config.ModKey != value.ToString())
            {
                _config.ModKey = value.ToString();
                OnPropertyChanged(nameof(ModKey));
            }
        }
    }

    public KEY Key
    {
        get => KeyExtension.GetKey(_config.Key);
        set
        {
            if (_config.Key != value.ToString())
            {
                _config.Key = value.ToString();
                OnPropertyChanged(nameof(Key));
            }
        }
    }

    public IEnumerable<EnumDisplayItem<MOD_KEY>> ModKeys
    {
        get
        {
            return Enum.GetValues(typeof(MOD_KEY))
                .Cast<MOD_KEY>()
                .Select(modKey => new EnumDisplayItem<MOD_KEY>(modKey, modKey.ToString()));
        }
    }

    public IEnumerable<EnumDisplayItem<KEY>> Keys
    {
        get
        {
            return Enum.GetValues(typeof(KEY))
                .Cast<KEY>()
                .Select(key => new EnumDisplayItem<KEY>(key, key.ToString()));
        }
    }

    private void SetPreviewMyName(string myName)
    {
        if (_previewMessage != null && _previewMessage.Username != myName)
        {
            _previewMessage = _previewMessage with { Username = myName };
            OnPropertyChanged(nameof(PreviewMessage));
        }
    }

    private void SetPreviewIconPath(string iconPath)
    {
        if (_previewMessage != null && _previewMessage.IconSource != iconPath)
        {
            _previewMessage = _previewMessage with { IconSource = iconPath };
            OnPropertyChanged(nameof(PreviewMessage));
        }
    }

    public event EventHandler RequestClose;

    public SettingsWindowViewModel()
    {
        SetPreviewMyName(MyName);
        SetPreviewIconPath(IconPath);

        SubmitCommand = new RelayCommand(Submit);
        CloseCommand = new RelayCommand(Close);
    }

    public ICommand SubmitCommand { get; }
    public ICommand CloseCommand { get; }
    public void Submit()
    {
        ServiceContainer.Instance.ConfigService.SaveConfig();
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    public void Close()
    {
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}