using System.Windows;
using System.Windows.Threading;

using Microsoft.Extensions.Logging;

namespace Medoz.KoeKan;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    
    public void HandleException(
            object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"Handled: {e.Exception.Message}");
        var logger = LoggerUtility.GetLoggerFactory().CreateLogger("UnhandledException");
        logger.LogError(e.Exception, "UnhandledException");
        e.Handled = true;
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
    }
}