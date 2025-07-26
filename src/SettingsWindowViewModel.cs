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
    private readonly IConfigService _configService;
    public string MyName
    {
        get => _configService.GetConfig().Username;
        set
        {
            if (_configService.GetConfig().Username != value)
            {
                _configService.GetConfig().Username = value;
                OnPropertyChanged(nameof(MyName));
            }
            SetPreviewMyName(value);
        }
    }

    public string IconPath
    {
        get => _configService.GetConfig().Icon ?? string.Empty;
        set
        {
            if (_configService.GetConfig().Icon != value)
            {
                _configService.GetConfig().Icon = value;
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
        get => _configService.GetConfig().Width;
        set
        {
            if (_configService.GetConfig().Width != value)
            {
                _configService.GetConfig().Width = value;
                OnPropertyChanged(nameof(Width));
            }
        }
    }

    public double Height
    {
        get => _configService.GetConfig().Height;
        set
        {
            if (_configService.GetConfig().Height != value)
            {
                _configService.GetConfig().Height = value;
                OnPropertyChanged(nameof(Height));
            }
        }
    }

    public double X
    {
        get => _configService.GetConfig().X;
        set
        {
            if (_configService.GetConfig().X != value)
            {
                _configService.GetConfig().X = value;
                OnPropertyChanged(nameof(X));
            }
        }
    }

    public double Y
    {
        get => _configService.GetConfig().Y;
        set
        {
            if (_configService.GetConfig().Y != value)
            {
                _configService.GetConfig().Y = value;
                OnPropertyChanged(nameof(Y));
            }
        }
    }

    public MOD_KEY ModKey
    {
        get => ModKeyExtension.GetModKey(_configService.GetConfig().ModKey);
        set
        {
            if (_configService.GetConfig().ModKey != value.ToString())
            {
                _configService.GetConfig().ModKey = value.ToString();
                OnPropertyChanged(nameof(ModKey));
            }
        }
    }

    public KEY Key
    {
        get => KeyExtension.GetKey(_configService.GetConfig().Key);
        set
        {
            if (_configService.GetConfig().Key != value.ToString())
            {
                _configService.GetConfig().Key = value.ToString();
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

    public event EventHandler? RequestClose;

    public SettingsWindowViewModel(IConfigService configService)
    {
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));

        // 初期化
        // コマンドの初期化
        SetPreviewMyName(MyName);
        SetPreviewIconPath(IconPath);

        SubmitCommand = new RelayCommand(Submit);
        CloseCommand = new RelayCommand(Close);
    }

    public ICommand SubmitCommand { get; }
    public ICommand CloseCommand { get; }
    public void Submit()
    {
        _configService.SaveConfig();
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