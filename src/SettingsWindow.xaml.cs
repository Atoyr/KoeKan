using System.Windows;

namespace Medoz.KoeKan;

public partial class SettingsWindow : Window
{
    public string Username { get; private set; } = "";

    public SettingsWindow()
    {
        InitializeComponent();

        // DataContextにバインドされたViewModelがDialogViewModelであることを前提に購読
        if (DataContext is SettingsWindowViewModel vm)
        {
            vm.RequestClose += (s, e) => this.Close();
        }
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }
}