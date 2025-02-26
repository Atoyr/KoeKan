using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Data;
using System.Windows.Threading;
using System.IO;

using Microsoft.Extensions.Logging;

using Medoz.KoeKan.Clients;
using Medoz.KoeKan.Data;
using Medoz.Logging;

namespace Medoz.KoeKan;

/// <summary>
/// Interaction logic for SettingsWindow.xaml
/// </summary>
public partial class SettingsWindowViewModel
{
    public Config Config { get; private set; }

    public SettingsWindowViewModel()
    {
        Config = Config.Load();
    }
}