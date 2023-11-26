using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Threading;

namespace Medoz.MessageTransporter;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
        public void HandleException(
            object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"Handled: {e.Exception.Message}");
            e.Handled = true;
        }
}