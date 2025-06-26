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
    private IConfigService _configService;
    public Config Config { get; private set; }

    public event EventHandler RequestClose;

    public SettingsWindowViewModel()
    {
        Config = new Config();
        SubmitCommand = new RelayCommand(Submit);
        SubmitAndCloseCommand = new RelayCommand(SubmitAndClose);
    }

    public SettingsWindowViewModel(IConfigService configService)
    {
        _configService = configService;
        Config = _configService.GetConfig();
        SubmitCommand = new RelayCommand(Submit);
        SubmitAndCloseCommand = new RelayCommand(SubmitAndClose);
    }
    public ICommand SubmitCommand { get; }
    public ICommand SubmitAndCloseCommand { get; }
    public void Submit()
    {
        _configService.Save();
        // Config.Save();
    }

    public void SubmitAndClose()
    {
        Submit();
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}