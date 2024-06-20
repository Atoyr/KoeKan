using System.Windows;

namespace Medoz.KoeKan;

public partial class SettingsWindow : Window
{
    public string Setting1 { get; private set; } = "";

    public SettingsWindow()
    {
        InitializeComponent();
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        Setting1 = Setting1TextBox.Text;
        DialogResult = true;
    }
}