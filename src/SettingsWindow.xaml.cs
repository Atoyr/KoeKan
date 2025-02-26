using System.Windows;

namespace Medoz.KoeKan;

public partial class SettingsWindow : Window
{
    public string Username { get; private set; } = "";

    public SettingsWindow()
    {
        InitializeComponent();
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        Username = UsernameTextBox.Text;
        DialogResult = true;
    }
}